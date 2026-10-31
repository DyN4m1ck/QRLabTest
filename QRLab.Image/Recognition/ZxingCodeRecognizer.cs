using System.Diagnostics;
using QRLab.Core.Interfaces.Recognition;
using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace QRLab.Image.Recognition;

// Реализация распознавания на ZXing BarcodeReaderGeneric. Ключевые аспекты:
// 1) последовательная обработка образцов с общей кнопкой отмены;
// 2) персональный тайм-аут на каждый образец (linked CancellationTokenSource);
// 3) ограничение числа одновременных декодеров семафором — защита от зависших нативных вызовов;
// 4) замер времени декодирования Stopwatch — попадает в CSV-отчёт.
public sealed class ZxingCodeRecognizer : ICodeRecognizer
{
    // Timed-out native/decoder work may finish later; keep the number of workers bounded.
    // Декодирование — CPU-тяжёлая и частично нативная операция; две параллельные попытки —
    // компромисс между скоростью и отзывчивостью UI. Тайм-аут не убивает поток, поэтому число
    // фоновых задач жёстко ограничено.
    private readonly SemaphoreSlim _workers = new(2);

    public async Task<IReadOnlyList<RecognitionPreview>> RecognizeAsync(IReadOnlyList<SamplePreview> samples,
        RecognitionOptions options, CancellationToken cancellationToken = default)
    {
        options.Validate();
        var results = new List<RecognitionPreview>(samples.Count);
        foreach (var sample in samples)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Засекаем время на весь проход по образцу: подготовка растра + декодирование.
            var elapsed = Stopwatch.StartNew();
            // Linked CTS объединяет внешнюю отмену и локальный дедлайн: сработает любая из причин.
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            // Индивидуальный тайм-аут — плохой код не должен блокировать обработку всего набора.
            deadline.CancelAfter(options.TimeoutMilliseconds);
            try
            {
                // Захват слота может не состояться до истечения дедлайна — тогда это тоже Timeout.
                await _workers.WaitAsync(deadline.Token);
                // Само декодирование уходит в ThreadPool, чтобы не блокировать UI-поток Avalonia.
                // CancellationToken.None внутри: отменить нативный вызов ZXing невозможно,
                // поэтому мы просто перестаём ждать результат, а слот освобождается в finally.
                var work = Task.Run(() =>
                {
                    try { return Decode(sample, options); }
                    finally { _workers.Release(); }
                }, CancellationToken.None);
                // WaitAsync бросает OperationCanceledException по тайм-ауту — задача продолжает
                // работать «в фоне» и корректно освобождает семафор, но её результат игнорируется.
                var decoded = await work.WaitAsync(deadline.Token);
                // Трёхзначная логика итога: ошибка/пусто -> Failed; совпало с ожидаемым текстом
                // образца -> Success; прочитали другое -> Mismatch (например, EAN исказил данные).
                var status = decoded.Error is not null || decoded.Content is null ? Status.Failed
                    : decoded.Content == sample.Content ? Status.Success : Status.Mismatch;
                results.Add(new(sample.Number, status, elapsed.Elapsed.TotalMilliseconds, decoded.Content, decoded.Error));
            }
            // Фильтр исключает случай «отменил пользователь»: тогда исключение уходит выше
            // и прерывает весь процесс. Сюда попадаем только по индивидуальному тайм-ауту.
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                results.Add(new(sample.Number, Status.Timeout, elapsed.Elapsed.TotalMilliseconds, Error: "Превышен тайм-аут декодирования."));
            }
        }
        return results;
    }

    // Ядро: превращает PNG образца в массив BGRA и вызывает ZXing. Возвращает кортеж,
    // чтобы отличить «код не найден» (Content=null) от исключения декодера (Error).
    private static (string? Content, string? Error) Decode(SamplePreview sample, RecognitionOptions options)
    {
        try
        {
            // Распознаём всегда последний кадр цепочки мутаций; если мутаций нет — исходное изображение.
            var source = sample.Frames.LastOrDefault()?.Image ?? sample.Image;
            using var image = SKImage.FromEncodedData(source.Data.Span);
            using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
            surface.Canvas.Clear(SKColors.White);
            surface.Canvas.DrawImage(image, 0, 0);
            using var pixels = surface.PeekPixels();
            // Плоский массив без выравнивания строк: ZXing ожидает ровно width*height*4 байта.
            var bytes = new byte[source.Width * source.Height * 4];
            var data = pixels.GetPixelSpan();
            for (var y = 0; y < source.Height; y++)
                // Построчное копирование убирает возможное выравнивание RowBytes из Skia-буфера.
                data.Slice(y * pixels.RowBytes, source.Width * 4).CopyTo(bytes.AsSpan(y * source.Width * 4));
            // Generic-ридер работает с произвольным LuminanceSource (в отличие от Bitmap-версии).
            // AutoRotate/TryHarder/TryInverted включаются только в «Enhanced» режиме:
            // он медленнее, но ловит перевёрнутые и повёрнутые коды после перспективы.
            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = options.Enhanced,
                Options = new DecodingOptions
                {
                    TryHarder = options.Enhanced,
                    TryInverted = options.Enhanced,
                        // Белый список форматов = ровно те, что умеет генерировать приложение;
                    // сужение поиска ускоряет декодирование и снижает ложные срабатывания.
                PossibleFormats = [BarcodeFormat.QR_CODE, BarcodeFormat.EAN_13, BarcodeFormat.CODE_128, BarcodeFormat.PDF_417]
                }
            };
            // ZXing оперирует яркостью (grayscale): конвертация BGRA->яркость происходит внутри источника.
            var luminance = new RGBLuminanceSource(bytes, source.Width, source.Height, RGBLuminanceSource.BitmapFormat.BGRA32);
            var result = reader.Decode(luminance);
            // null результата — штатная ситуация (мутация уничтожила код), а не исключение.
            return result is null ? (null, "Код не найден.") : (result.Text, null);
        }
        // Повреждённые данные или ошибки растра фиксируются как Error конкретного образца —
        // весь батч распознавания не должен падать из-за одного битого файла.
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return (null, exception.Message);
        }
    }
}