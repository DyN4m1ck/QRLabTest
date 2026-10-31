using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;

namespace QRLab.Core.Interfaces.Recognition;

/// <summary>
/// Абстракция распознавателя кодов: декодирует содержимое с готовых изображений.
/// Реализация в QRLab.Image построена поверх ZXing; интерфейс позволяет заменить её
/// другой библиотекой или заглушкой без изменения ViewModel.
/// </summary>
public interface ICodeRecognizer
{
    /// <summary>
    /// Асинхронно распознаёт список образцов.
    /// Для каждого образца возвращается <see cref="RecognitionPreview"/> со статусом
    /// (успех, ошибка, тайм-аут, несовпадение), временем декодирования и считанным текстом.
    /// Поддерживается отмена через <paramref name="cancellationToken"/>.
    /// </summary>
    Task<IReadOnlyList<RecognitionPreview>> RecognizeAsync(IReadOnlyList<SamplePreview> samples, RecognitionOptions options, CancellationToken cancellationToken = default);
}
