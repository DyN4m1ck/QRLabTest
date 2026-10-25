namespace QRLab.Core.Models.Recognition;

public sealed record RecognitionPreview(int Number, Status Status, double Milliseconds, string? DecodedContent = null, string? Error = null);