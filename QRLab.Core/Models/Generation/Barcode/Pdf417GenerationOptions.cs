using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// Параметры генерации двумерного stacked-кода PDF417.
/// Не является линейным: изображение масштабируется пропорционально, как у QR.
public sealed record Pdf417GenerationOptions : BarcodeGenerationOptions
{
    /// Вид кода — PDF417.
    public override BarcodeType BarcodeType => BarcodeType.Pdf417;
}
