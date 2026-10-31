namespace QRLab.Core.Models.Generation.Qrcode;

/// <summary>Уровень коррекции ошибок QR-кода (по стандарту ISO/IEC 18004).</summary>
public enum QrCorrectionLevel
{
    /// <summary>L — восстанавливается ~7% кодовых слов.</summary>
    Low,
    /// <summary>M — восстанавливается ~15% кодовых слов.</summary>
    Medium,
    /// <summary>Q — восстанавливается ~25% кодовых слов.</summary>
    Quartile,
    /// <summary>H — восстанавливается ~30% кодовых слов (лучше всего для мутаций).</summary>
    High
}
