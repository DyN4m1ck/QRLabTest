namespace QRLab.Core.Models.Recognition;

public sealed record RecognitionOptions(int TimeoutMilliseconds = RecognitionDefaults.TimeoutMilliseconds, bool Enhanced = RecognitionDefaults.Enhanced)
{
    public void Validate()
    {
        if (TimeoutMilliseconds < RecognitionDefaults.MinimumTimeoutMilliseconds || TimeoutMilliseconds > RecognitionDefaults.MaximumTimeoutMilliseconds)
            throw new ArgumentException($"Тайм-аут должен быть от {RecognitionDefaults.MinimumTimeoutMilliseconds} до {RecognitionDefaults.MaximumTimeoutMilliseconds} мс.");
    }
}