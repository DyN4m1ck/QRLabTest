namespace QRLab.Core.Models;

/// Устаревшая модель формата кода. Используется только legacy-классами
/// CodeSample/MutationStep и тестом ModelTests; рабочий код приложения
/// опирается на CodeKind и BarcodeType из пространства имён Generation.
public enum Format { QrCode, Barcode }
