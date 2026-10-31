namespace QRLab.Core.Models.Generation.Qrcode;

/// Параметры генерации QR-кода: версия и уровень коррекции поверх общих настроек.
public sealed record QrGenerationOptions : GenerationOptions
{
    /// Версия QR (1..40); 0 — автоматический выбор по объёму данных.
    public int Version { get; init; } = GenerationDefaults.AutomaticQrVersion;
    /// Уровень коррекции ошибок L/M/Q/H.
    public QrCorrectionLevel Correction { get; init; } = GenerationDefaults.Correction;
    /// Род кода — QR.
    public override CodeKind Kind => CodeKind.QrCode;
    /// Префикс имён образцов.
    public override string SamplePrefix => "QR";

    /// Проверяет общие параметры и специфичные для QR (версия, коррекция).
    public override void Validate()
    {
        base.Validate();
        if (Version < GenerationDefaults.AutomaticQrVersion || Version > GenerationLimits.MaximumQrVersion)
            throw new ArgumentException($"Класс размера QR должен быть от 1 до {GenerationLimits.MaximumQrVersion} (0 — авто).");
        if (!Enum.IsDefined(Correction)) throw new ArgumentException("Неизвестный уровень коррекции QR.");
    }
}
