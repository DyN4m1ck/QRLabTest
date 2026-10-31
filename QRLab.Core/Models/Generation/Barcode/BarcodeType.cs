namespace QRLab.Core.Models.Generation.Barcode;

/// Поддерживаемые виды штрихкодов.
public enum BarcodeType
{
    /// Линейный европейский 13-значный код с контрольной суммой.
    Ean13,
    /// Линейный алфавитно-цифровой код произвольной длины.
    Code128,
    /// Двумерный stacked-код PDF417.
    Pdf417
}
