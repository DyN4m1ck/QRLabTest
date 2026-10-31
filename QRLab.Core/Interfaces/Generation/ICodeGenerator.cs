using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Samples;

namespace QRLab.Core.Interfaces.Generation;

/// <summary>
/// Абстракция генератора штрих- и QR-кодов.
/// Остальные слои зависят только от этого интерфейса и не знают о конкретной
/// библиотеке кодирования (ZXing), что позволяет подменять реализацию в тестах.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Создаёт набор образцов по параметрам генерации.
    /// Сначала проверяются параметры (<see cref="GenerationOptions.Validate"/>), затем
    /// генерируется ровно <see cref="GenerationOptions.Count"/> изображений.
    /// При некорректных параметрах выбрасывается <see cref="ArgumentException"/>.
    /// </summary>
    IReadOnlyList<SamplePreview> CreateSamples(GenerationOptions options);
}
