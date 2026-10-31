using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Параметры генерации двумерного stacked-кода PDF417.
/// Не является линейным: изображение масштабируется пропорционально, как у QR.
/// </summary>
public sealed record Pdf417GenerationOptions : BarcodeGenerationOptions
{
    /// <summary>Вид кода — PDF417.</summary>
    public override BarcodeType BarcodeType => BarcodeType.Pdf417;
}
