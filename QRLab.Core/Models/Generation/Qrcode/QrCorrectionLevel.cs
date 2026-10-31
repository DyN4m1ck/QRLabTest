namespace QRLab.Core.Models.Generation.Qrcode;

/// Уровень коррекции ошибок QR-кода (по стандарту ISO/IEC 18004).
public enum QrCorrectionLevel
{
    /// L — восстанавливается ~7% кодовых слов.
    Low,
    /// M — восстанавливается ~15% кодовых слов.
    Medium,
    /// Q — восстанавливается ~25% кодовых слов.
    Quartile,
    /// H — восстанавливается ~30% кодовых слов (лучше всего для мутаций).
    High
}
