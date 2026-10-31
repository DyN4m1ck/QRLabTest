using QRLab.Core.Interfaces.Generation;
using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Samples;
using QRLab.Image.Generation.Encoders;
using ZXing;

namespace QRLab.Image.Generation;

// Конкретная реализация ICodeGenerator из слоя Core. Это «дирижёр» процесса создания образцов:
// он сам ничего не кодирует и не рисует, а связывает три шага — подготовка содержимого,
// кодирование в битовую матрицу (ZXing) и растеризация матрицы в PNG (SkiaSharp).
public sealed class CodeGenerator : ICodeGenerator
{
    // Список кодировщиков-стратегий (QR, штрихкоды). Выбор конкретного экземпляра
    // происходит по результату Supports(options), поэтому генератор не зависит от типа опций.
    private readonly IReadOnlyList<CodeEncoder> _encoders;

    // Единый renderer переиспользуется между образцами: он не хранит состояния.
    private readonly CodeImageRenderer _renderer = new();

    // Конструктор по умолчанию регистрирует встроенные кодировщики.
    public CodeGenerator() : this([new QrCodeEncoder(), new BarcodeEncoder()]) { }

    // Перегрузка для тестов и расширений: можно передать собственный набор кодировщиков.
    public CodeGenerator(IEnumerable<CodeEncoder> encoders) => _encoders = encoders.ToArray();

    // Публичная точка входа: по параметрам создаёт ровно options.Count образцов.
    public IReadOnlyList<SamplePreview> CreateSamples(GenerationOptions options)
    {
        // Сначала отбрасываем заведомо некорректные параметры (диапазоны, обязательные поля).
        options.Validate();

        // Ищем первый кодировщик, который «понимает» переданные опции.
        var encoder = _encoders.FirstOrDefault(e => e.Supports(options))
            ?? throw new ArgumentException("Кодировщик для выбранного типа кода не зарегистрирован.");

        var result = new List<SamplePreview>(options.Count);
        for (var index = 0; index < options.Count; index++)
        {
            // checked защищает от переполнения int при нумерации образцов.
            var number = checked(options.FirstNumber + index);
            try
            {
                // Содержимое может быть общим для всех образцов или уникальным (например, с подстановкой номера).
                var content = options.PrepareContent(number);

                // Кодируем текст в логическую матрицу модулей (чёрное/белое), ещё без пикселей.
                var matrix = encoder.Encode(content, options);

                // Если модулей больше, чем запрошенных пикселей, масштаб < 1 невозможен —
                // просим пользователя увеличить размер изображения.
                if (matrix.Width > options.Width || matrix.Height > options.Height)
                    throw new ArgumentException($"Увеличьте размер изображения минимум до {matrix.Width} × {matrix.Height} пикселей.");

                // Превращаем матрицу в реальный PNG заданного размера.
                var image = _renderer.Render(matrix, options);

                // Новый образец создаётся без кадров мутаций (пустой список) — их добавит позже
                // отдельный экран «Мутации». Параметры генерации сохраняются вместе с образцом.
                result.Add(new SamplePreview(number, options.GetSampleName(number), content, image, [], options));
            }
            // Ошибки ZXing (WriterException) и ошибки валидации оборачиваем в единый формат,
            // чтобы пользователь понял, на каком именно образце всё сломалось.
            catch (Exception exception) when (exception is ArgumentException or WriterException)
            {
                throw new ArgumentException($"Образец {number}: {exception.Message}", exception);
            }
        }
        return result;
    }
}
