using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Generation.Barcode;
using ZXing;

namespace QRLab.Image.Generation.Encoders;

// Кодировщик семейства штрихкодов (EAN-13, Code128, PDF417). Один класс на все подтипы:
// различие сводится к сопоставлению BarcodeType -> BarcodeFormat через таблицу Formats.
public sealed class BarcodeEncoder : CodeEncoder
{
    // Статическая таблица соответствия: модель приложения -> формат ZXing.
    // При добавлении нового типа достаточно дописать сюда пару и класс опций в Core.
    private static readonly Dictionary<BarcodeType, BarcodeFormat> Formats = new()
    {
        [BarcodeType.Ean13] = BarcodeFormat.EAN_13,
        [BarcodeType.Code128] = BarcodeFormat.CODE_128,
        [BarcodeType.Pdf417] = BarcodeFormat.PDF_417
    };

    // Поддержка = «это BarcodeGenerationOptions И тип есть в таблице» —
    // неизвестный подтип приведёт к понятной ошибке «кодировщик не зарегистрирован».
    public override bool Supports(GenerationOptions options) => 
        options is BarcodeGenerationOptions barcode 
        && Formats.ContainsKey(barcode.BarcodeType);
    
    // Простое отображение по таблице; вызывается только после успешного Supports.
    protected override BarcodeFormat GetFormat(GenerationOptions options) 
        => Formats[((BarcodeGenerationOptions) options).BarcodeType];

    // Из специфичных подсказок — только UTF-8 для PDF417 (он поддерживает произвольный текст);
    // EAN-13 и Code128 работают с фиксированными алфавитами, где кодировка не важна.
    protected override IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options)
    {
        var hints = base.CreateHints(options);
        if (options is Pdf417GenerationOptions) hints[EncodeHintType.CHARACTER_SET] = "UTF-8";
        return hints;
    }
}
