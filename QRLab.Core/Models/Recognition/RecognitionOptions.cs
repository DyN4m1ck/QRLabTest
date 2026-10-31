namespace QRLab.Core.Models.Recognition;

/// <summary>Параметры одного прогона распознавания: лимит времени и режим качества.</summary>
public sealed record RecognitionOptions(
    int TimeoutMilliseconds = RecognitionDefaults.TimeoutMilliseconds,
    bool Enhanced = RecognitionDefaults.Enhanced)
{
    /// <summary>Проверяет, что тайм-аут лежит в допустимом диапазоне.</summary>
    public void Validate()
    {
        if (TimeoutMilliseconds < RecognitionDefaults.MinimumTimeoutMilliseconds || TimeoutMilliseconds > RecognitionDefaults.MaximumTimeoutMilliseconds)
            throw new ArgumentException($"Тайм-аут должен быть от {RecognitionDefaults.MinimumTimeoutMilliseconds} до {RecognitionDefaults.MaximumTimeoutMilliseconds} мс.");
    }
}
