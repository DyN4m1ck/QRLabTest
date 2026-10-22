using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

public sealed record Pdf417GenerationOptions : BarcodeGenerationOptions
{
    public override BarcodeType BarcodeType => BarcodeType.Pdf417;
}
