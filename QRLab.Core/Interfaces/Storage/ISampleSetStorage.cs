using QRLab.Core.Models.Samples;
using QRLab.Core.Models.Storage;

namespace QRLab.Core.Interfaces.Storage;

/// Хранилище наборов образцов: сериализует их на диск и читает обратно.
/// Формат хранения — папка с PNG-изображениями и JSON-манифестом (qrlab-set.json).
public interface ISampleSetStorage
{
    /// Сохраняет набор образцов в папку, указанную в <paramref name="options"/>.
    /// Реализация обязана писать данные атомарно (через staging-каталог) и никогда
    /// не перезаписывать существующие наборы. Возвращает сохранённый <see cref="SampleSet"/>.
    SampleSet Save(IReadOnlyList<SamplePreview> samples, ExportOptions options);

    /// Загружает ранее сохранённый набор из папки, полностью проверяя манифест
    /// и изображения. При повреждённых данных выбрасывается <see cref="InvalidDataException"/>.
    SampleSet Load(string directory);
}
