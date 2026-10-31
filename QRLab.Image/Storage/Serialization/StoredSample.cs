using QRLab.Core.Models.Generation;

namespace QRLab.Image.Storage.Serialization;

// DTO образца в манифесте: метаданные, полиморфные настройки генерации, имя файла оригинала
// и массив кадров мутаций. Изображения в JSON не вкладываются — только ссылки на файлы.
internal sealed record StoredSample(int Number, string Name, string Content, GenerationOptions Settings,
    string Image, StoredMutation[] Frames);