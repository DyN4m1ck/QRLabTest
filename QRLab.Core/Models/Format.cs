namespace QRLab.Core.Models;

/// <summary>
/// Устаревшая модель формата кода. Используется только legacy-классами
/// CodeSample/MutationStep и тестом ModelTests; рабочий код приложения
/// опирается на CodeKind и BarcodeType из пространства имён Generation.
/// </summary>
public enum Format { QrCode, Barcode }
