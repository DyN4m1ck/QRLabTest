namespace QRLab.Core.Models.Generation;

public sealed record Code128GenerationOptions : LinearBarcodeGenerationOptions
{
    public override BarcodeType BarcodeType => BarcodeType.Code128;
}
