using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// Параметры генерации линейного штрихкода Code 128.
public sealed record Code128GenerationOptions : LinearBarcodeGenerationOptions
{
    /// Вид кода — Code 128.
    public override BarcodeType BarcodeType => BarcodeType.Code128;
}
