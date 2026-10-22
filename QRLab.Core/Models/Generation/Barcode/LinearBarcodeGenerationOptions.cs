using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

public abstract record LinearBarcodeGenerationOptions : BarcodeGenerationOptions
{
    public override bool IsLinear => true;
}
