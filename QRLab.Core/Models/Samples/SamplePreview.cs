using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Core.Models.Samples;

/// <summary>
/// Полное описание одного образца: номер, имя, закодированное содержимое,
/// исходное PNG-изображение, кадры мутаций и настройки, которыми он создан.
/// Это центральная сквозная модель между генерацией, редактированием,
/// распознаванием и хранилищем.
/// </summary>
public sealed record SamplePreview(
    int Number,
    string Name,
    string Content,
    RasterImage Image,
    IReadOnlyList<MutationFrame> Frames,
    GenerationOptions Settings
    );
