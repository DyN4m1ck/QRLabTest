using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Generation.Qrcode;
using ZXing;
using ZXing.QrCode.Internal;

namespace QRLab.Image.Generation.Encoders;

// Кодировщик QR-кодов: единственный источник подсказок про коррекцию ошибок и версию символа.
public sealed class QrCodeEncoder : CodeEncoder
{
    // Активируется, когда UI прислал именно QrGenerationOptions (полиморфная модель из Core).
    public override bool Supports(GenerationOptions options) => options is QrGenerationOptions;
    // Жёстко фиксируем формат ZXing для QR.
    protected override BarcodeFormat GetFormat(GenerationOptions options) => BarcodeFormat.QR_CODE;

    // Расширяем базовые подсказки спецификой QR: кодировка UTF-8, уровень коррекции ошибок
    // и (опционально) явная версия символа вместо автоматического подбора.
    protected override IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options)
    {
        var qr = (QrGenerationOptions)options;
        var hints = base.CreateHints(options);
        // Явно указываем UTF-8, чтобы кириллица и эмодзи кодировались предсказуемо.
        hints[EncodeHintType.CHARACTER_SET] = "UTF-8";
        // Уровень коррекции L/M/Q/H переводится из модели приложения в enum ZXing:
        // чем выше уровень, тем больше избыточных данных и тем устойчивее код к мутациям.
        hints[EncodeHintType.ERROR_CORRECTION] = qr.Correction switch
        {
            QrCorrectionLevel.Low => ErrorCorrectionLevel.L,
            QrCorrectionLevel.Medium => ErrorCorrectionLevel.M,
            QrCorrectionLevel.Quartile => ErrorCorrectionLevel.Q,
            QrCorrectionLevel.High => ErrorCorrectionLevel.H,
            _ => throw new ArgumentOutOfRangeException(nameof(options))
        };
        // Специальная версия-«авто» не передаётся в подсказки — тогда размер выбирает сам ZXing.
        if (qr.Version != GenerationDefaults.AutomaticQrVersion) hints[EncodeHintType.QR_VERSION] = qr.Version;
        return hints;
    }
}
