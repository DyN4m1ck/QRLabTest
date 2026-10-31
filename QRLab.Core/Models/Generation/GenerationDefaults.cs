using QRLab.Core.Models.Generation.Barcode;
using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Значения параметров генерации по умолчанию. Единый источник истины для
/// конструктора записей GenerationOptions, UI-форм и тестов сверки значений.
/// </summary>
public static class GenerationDefaults
{
    /// <summary>Шаблон содержимого по умолчанию.</summary>
    public const string Template = "LAB-2026-{num}";
    /// <summary>Плейсхолдер номера внутри шаблона.</summary>
    public const string NumberPlaceholder = "{num}";
    /// <summary>Формат номера: четыре цифры с ведущими нулями.</summary>
    public const string NumberFormat = "0000";
    /// <summary>Номер первого образца.</summary>
    public const int FirstNumber = 1;
    /// <summary>Размер пакета по умолчанию.</summary>
    public const int Count = 120;
    /// <summary>Количество образцов в режиме «один код».</summary>
    public const int SingleCount = 1;
    /// <summary>Ширина изображения по умолчанию, px.</summary>
    public const int Width = 360;
    /// <summary>Высота изображения по умолчанию, px.</summary>
    public const int Height = 360;
    /// <summary>Тихая зона по умолчанию, модули.</summary>
    public const int QuietZone = 2;
    /// <summary>0 означает автоматический выбор версии QR.</summary>
    public const int AutomaticQrVersion = 0;
    /// <summary>Уровень коррекции по умолчанию (H — устойчив к искажениям).</summary>
    public const QrCorrectionLevel Correction = QrCorrectionLevel.High;
    /// <summary>Вид штрихкода по умолчанию.</summary>
    public const BarcodeType Barcode = BarcodeType.Code128;
    /// <summary>Режим генерации по умолчанию.</summary>
    public const GenerationMode Mode = GenerationMode.Batch;
    /// <summary>Вид кода по умолчанию.</summary>
    public const CodeKind Kind = CodeKind.QrCode;
    /// <summary>Источник содержимого по умолчанию.</summary>
    public const ContentSource Source = ContentSource.NumberedTemplate;
    /// <summary>Имя нового набора, если пользователь не загрузил сохранённый.</summary>
    public const string NewSetName = "Набор";
}
