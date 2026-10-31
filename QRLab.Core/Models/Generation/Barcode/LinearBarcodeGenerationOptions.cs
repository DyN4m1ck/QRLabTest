using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// Промежуточная запись для линейных (одномерных) штрихкодов: включает флаг
/// IsLinear, из-за которого рендер растягивает код по высоте, а не сохраняет квадрат.
/// PDF417 остаётся «нелинейным» и масштабируется как QR.
public abstract record LinearBarcodeGenerationOptions : BarcodeGenerationOptions
{
    /// Признак линейного кода для renderer'а и подсказок ZXing.
    public override bool IsLinear => true;
}
