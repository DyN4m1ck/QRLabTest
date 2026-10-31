namespace QRLab.Core.Models.Generation.Barcode;

/// Параметры генерации штрихкода EAN-13. Переопределяет PrepareContent:
/// в пакетном режиме номер дополняется нулями до 12 цифр (контрольную цифру
/// ZXing вычисляет сам), а в текстовом режиме содержимое проверяется на
/// «чистые» ASCII-цифры нужной длины — иначе EAN-кодировщик ZXing принимает
/// и русские буквы, и полноширинные цифры, выдавая некорректный код.
public sealed record Ean13GenerationOptions : LinearBarcodeGenerationOptions
{
    /// Вид кода — EAN-13.
    public override BarcodeType BarcodeType => BarcodeType.Ean13;

    /// Допустимая длина данных EAN-13: 12 или 13 цифр.
    private const int DigitsLength = 13;

    /// Готовит содержимое: для шаблона с номером возвращает 12 цифр,
    /// для произвольного текста — предварительно проверенную строку.
    public override string PrepareContent(int number) => Source == ContentSource.NumberedTemplate
        ? number.ToString("D12", System.Globalization.CultureInfo.InvariantCulture)
        : ValidateText(Template);

    /// Проверяет, что текст — ровно 12 или 13 обычных ASCII-цифр.
    private static string ValidateText(string text)
    {
        if (text.Length is not (DigitsLength - 1 or DigitsLength) || !text.All(char.IsAsciiDigit))
            throw new ArgumentException("Содержимое EAN-13 должно содержать 12 или 13 арабских цифр.");
        return text;
    }
}
