using QRLab.Core.Models.Samples;
using QRLab.Core.Models.Storage;

namespace QRLab.Core.Interfaces.Storage;

/// <summary>
/// Хранилище наборов образцов: сериализует их на диск и читает обратно.
/// Формат хранения — папка с PNG-изображениями и JSON-манифестом (qrlab-set.json).
/// </summary>
public interface ISampleSetStorage
{
    /// <summary>
    /// Сохраняет набор образцов в папку, указанную в <paramref name="options"/>.
    /// Реализация обязана писать данные атомарно (через staging-каталог) и никогда
    /// не перезаписывать существующие наборы. Возвращает сохранённый <see cref="SampleSet"/>.
    /// </summary>
    SampleSet Save(IReadOnlyList<SamplePreview> samples, ExportOptions options);

    /// <summary>
    /// Загружает ранее сохранённый набор из папки, полностью проверяя манифест
    /// и изображения. При повреждённых данных выбрасывается <see cref="InvalidDataException"/>.
    /// </summary>
    SampleSet Load(string directory);
}
