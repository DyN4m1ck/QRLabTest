using System.Diagnostics;
using QRLab.Core.Interfaces.Recognition;
using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace QRLab.Image.Recognition;

public sealed class ZxingCodeRecognizer : ICodeRecognizer
{
    // Timed-out native/decoder work may finish later; keep the number of workers bounded.
    private readonly SemaphoreSlim _workers = new(2);

    public async Task<IReadOnlyList<RecognitionPreview>> RecognizeAsync(IReadOnlyList<SamplePreview> samples,
        RecognitionOptions options, CancellationToken cancellationToken = default)
    {
        options.Validate();
        var results = new List<RecognitionPreview>(samples.Count);
        foreach (var sample in samples)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var elapsed = Stopwatch.StartNew();
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(options.TimeoutMilliseconds);
            try
            {
                await _workers.WaitAsync(deadline.Token);
                var work = Task.Run(() =>
                {
                    try { return Decode(sample, options); }
                    finally { _workers.Release(); }
                }, CancellationToken.None);
                var decoded = await work.WaitAsync(deadline.Token);
                var status = decoded.Error is not null || decoded.Content is null ? Status.Failed
                    : decoded.Content == sample.Content ? Status.Success : Status.Mismatch;
                results.Add(new(sample.Number, status, elapsed.Elapsed.TotalMilliseconds, decoded.Content, decoded.Error));
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                results.Add(new(sample.Number, Status.Timeout, elapsed.Elapsed.TotalMilliseconds, Error: "Превышен тайм-аут декодирования."));
            }
        }
        return results;
    }

    private static (string? Content, string? Error) Decode(SamplePreview sample, RecognitionOptions options)
    {
        try
        {
            var source = sample.Frames.LastOrDefault()?.Image ?? sample.Image;
            using var image = SKImage.FromEncodedData(source.Data.Span);
            using var surface = SKSurface.Create(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
            surface.Canvas.Clear(SKColors.White);
            surface.Canvas.DrawImage(image, 0, 0);
            using var pixels = surface.PeekPixels();
            var bytes = new byte[source.Width * source.Height * 4];
            var data = pixels.GetPixelSpan();
            for (var y = 0; y < source.Height; y++)
                data.Slice(y * pixels.RowBytes, source.Width * 4).CopyTo(bytes.AsSpan(y * source.Width * 4));
            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = options.Enhanced,
                Options = new DecodingOptions
                {
                    TryHarder = options.Enhanced,
                    TryInverted = options.Enhanced,
                    PossibleFormats = [BarcodeFormat.QR_CODE, BarcodeFormat.EAN_13, BarcodeFormat.CODE_128, BarcodeFormat.PDF_417]
                }
            };
            var luminance = new RGBLuminanceSource(bytes, source.Width, source.Height, RGBLuminanceSource.BitmapFormat.BGRA32);
            var result = reader.Decode(luminance);
            return result is null ? (null, "Код не найден.") : (result.Text, null);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return (null, exception.Message);
        }
    }
}