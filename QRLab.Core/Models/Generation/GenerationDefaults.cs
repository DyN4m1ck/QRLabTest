using QRLab.Core.Models.Generation.Barcode;
using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// Значения параметров генерации по умолчанию. Единый источник истины для
/// конструктора записей GenerationOptions, UI-форм и тестов сверки значений.
public static class GenerationDefaults
{
    /// Шаблон содержимого по умолчанию.
    public const string Template = "LAB-2026-{num}";
    /// Плейсхолдер номера внутри шаблона.
    public const string NumberPlaceholder = "{num}";
    /// Формат номера: четыре цифры с ведущими нулями.
    public const string NumberFormat = "0000";
    /// Номер первого образца.
    public const int FirstNumber = 1;
    /// Размер пакета по умолчанию.
    public const int Count = 120;
    /// Количество образцов в режиме «один код».
    public const int SingleCount = 1;
    /// Ширина изображения по умолчанию, px.
    public const int Width = 360;
    /// Высота изображения по умолчанию, px.
    public const int Height = 360;
    /// Тихая зона по умолчанию, модули.
    public const int QuietZone = 2;
    /// 0 означает автоматический выбор версии QR.
    public const int AutomaticQrVersion = 0;
    /// Уровень коррекции по умолчанию (H — устойчив к искажениям).
    public const QrCorrectionLevel Correction = QrCorrectionLevel.High;
    /// Вид штрихкода по умолчанию.
    public const BarcodeType Barcode = BarcodeType.Code128;
    /// Режим генерации по умолчанию.
    public const GenerationMode Mode = GenerationMode.Batch;
    /// Вид кода по умолчанию.
    public const CodeKind Kind = CodeKind.QrCode;
    /// Источник содержимого по умолчанию.
    public const ContentSource Source = ContentSource.NumberedTemplate;
    /// Имя нового набора, если пользователь не загрузил сохранённый.
    public const string NewSetName = "Набор";
}
