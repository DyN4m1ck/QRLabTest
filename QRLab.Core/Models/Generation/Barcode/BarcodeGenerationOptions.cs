namespace QRLab.Core.Models.Generation.Barcode;

/// Базовая запись для всех штрихкодов. Хранит конкретный вид кода и предоставляет
/// фабричный метод Create, сопоставляющий enum BarcodeType с нужным подтипом опций.
public abstract record BarcodeGenerationOptions : GenerationOptions
{
    /// Все штрихкоды относятся к роду Barcode.
    public override CodeKind Kind => CodeKind.Barcode;
    /// Общий префикс имён образцов для штрихкодов.
    public override string SamplePrefix => "BAR";
    /// Конкретный вид штрихкода (EAN-13, Code 128, PDF417).
    public abstract BarcodeType BarcodeType { get; }

    /// Фабрика: создаёт набор параметров по выбранному виду штрихкода.
    public static BarcodeGenerationOptions Create(BarcodeType type) => type switch
    {
        BarcodeType.Ean13 => new Ean13GenerationOptions(),
        BarcodeType.Code128 => new Code128GenerationOptions(),
        BarcodeType.Pdf417 => new Pdf417GenerationOptions(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
