namespace QRLab.Core.Models.Recognition;

/// Параметры одного прогона распознавания: лимит времени и режим качества.
public sealed record RecognitionOptions(
    int TimeoutMilliseconds = RecognitionDefaults.TimeoutMilliseconds,
    bool Enhanced = RecognitionDefaults.Enhanced)
{
    /// Проверяет, что тайм-аут лежит в допустимом диапазоне.
    public void Validate()
    {
        if (TimeoutMilliseconds < RecognitionDefaults.MinimumTimeoutMilliseconds || TimeoutMilliseconds > RecognitionDefaults.MaximumTimeoutMilliseconds)
            throw new ArgumentException($"Тайм-аут должен быть от {RecognitionDefaults.MinimumTimeoutMilliseconds} до {RecognitionDefaults.MaximumTimeoutMilliseconds} мс.");
    }
}
