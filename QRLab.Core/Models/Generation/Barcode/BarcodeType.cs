namespace QRLab.Core.Models.Generation.Barcode;

/// <summary>Поддерживаемые виды штрихкодов.</summary>
public enum BarcodeType
{
    /// <summary>Линейный европейский 13-значный код с контрольной суммой.</summary>
    Ean13,
    /// <summary>Линейный алфавитно-цифровой код произвольной длины.</summary>
    Code128,
    /// <summary>Двумерный stacked-код PDF417.</summary>
    Pdf417
}
