using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// <summary>Параметры генерации линейного штрихкода Code 128.</summary>
public sealed record Code128GenerationOptions : LinearBarcodeGenerationOptions
{
    /// <summary>Вид кода — Code 128.</summary>
    public override BarcodeType BarcodeType => BarcodeType.Code128;
}
