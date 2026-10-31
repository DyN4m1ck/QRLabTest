using QRLab.Core.Models.Generation;
using ZXing;
using ZXing.Common;

namespace QRLab.Image.Generation.Encoders;

// Базовый класс стратегии кодирования: инкапсулирует работу с ZXing MultiFormatWriter.
// Потомки отвечают за конкретное семейство форматов (QR или линейные/PDF417 штрихкоды),
// описывая, какие опции они поддерживают и какой BarcodeFormat использовать.
public abstract class CodeEncoder
{
    // Фиксированная высота матрицы линейного штрихкода: у таких кодов ширина определяется
    // содержимым, а высота произвольна — renderer потом растянет её на весь кадр.
    protected const int LinearMatrixHeight = 64;
    // Признак «этот кодировщик умеет работать с данными опциями» — используется генератором
    // для выбора стратегии без проверки типов снаружи.
    public abstract bool Supports(GenerationOptions options);

    // Возвращает формат ZXing, соответствующий настройкам пользователя.
    protected abstract BarcodeFormat GetFormat(GenerationOptions options);

    // Подсказки ZXing по умолчанию: только размер защитного поля (quiet zone).
    // Для линейных кодов поле удваивается, т.к. EAN/Code128 требуют широких полей для сканеров.
    protected virtual IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options) =>
        new Dictionary<EncodeHintType, object>
        {
            [EncodeHintType.MARGIN] = options.IsLinear ? options.QuietZone * 2 : options.QuietZone
        };

    // Кодирование текста в битовую матрицу. Ноль в качестве желаемого размера означает
    // «подобрать минимально возможный автоматом» — реальный пиксельный размер задаст renderer.
    public BitMatrix Encode(string content, GenerationOptions options) => new MultiFormatWriter().encode(
        content, GetFormat(options), 0, options.IsLinear ? LinearMatrixHeight : 0, CreateHints(options));
}
