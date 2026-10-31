namespace QRLab.Core.Models.Generation.Qrcode;

/// <summary>Параметры генерации QR-кода: версия и уровень коррекции поверх общих настроек.</summary>
public sealed record QrGenerationOptions : GenerationOptions
{
    /// <summary>Версия QR (1..40); 0 — автоматический выбор по объёму данных.</summary>
    public int Version { get; init; } = GenerationDefaults.AutomaticQrVersion;
    /// <summary>Уровень коррекции ошибок L/M/Q/H.</summary>
    public QrCorrectionLevel Correction { get; init; } = GenerationDefaults.Correction;
    /// <summary>Род кода — QR.</summary>
    public override CodeKind Kind => CodeKind.QrCode;
    /// <summary>Префикс имён образцов.</summary>
    public override string SamplePrefix => "QR";

    /// <summary>Проверяет общие параметры и специфичные для QR (версия, коррекция).</summary>
    public override void Validate()
    {
        base.Validate();
        if (Version < GenerationDefaults.AutomaticQrVersion || Version > GenerationLimits.MaximumQrVersion)
            throw new ArgumentException($"Класс размера QR должен быть от 1 до {GenerationLimits.MaximumQrVersion} (0 — авто).");
        if (!Enum.IsDefined(Correction)) throw new ArgumentException("Неизвестный уровень коррекции QR.");
    }
}
