namespace QRLab.Core.Models.Generation.Barcode;

/// <summary>
/// Базовая запись для всех штрихкодов. Хранит конкретный вид кода и предоставляет
/// фабричный метод Create, сопоставляющий enum BarcodeType с нужным подтипом опций.
/// </summary>
public abstract record BarcodeGenerationOptions : GenerationOptions
{
    /// <summary>Все штрихкоды относятся к роду Barcode.</summary>
    public override CodeKind Kind => CodeKind.Barcode;
    /// <summary>Общий префикс имён образцов для штрихкодов.</summary>
    public override string SamplePrefix => "BAR";
    /// <summary>Конкретный вид штрихкода (EAN-13, Code 128, PDF417).</summary>
    public abstract BarcodeType BarcodeType { get; }

    /// <summary>Фабрика: создаёт набор параметров по выбранному виду штрихкода.</summary>
    public static BarcodeGenerationOptions Create(BarcodeType type) => type switch
    {
        BarcodeType.Ean13 => new Ean13GenerationOptions(),
        BarcodeType.Code128 => new Code128GenerationOptions(),
        BarcodeType.Pdf417 => new Pdf417GenerationOptions(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
