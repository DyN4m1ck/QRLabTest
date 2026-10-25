namespace QRLab.Core.Models.Recognition;

public sealed record RecognitionReportRow(int Number, string Name, string Format, string ExpectedContent,
    string? DecodedContent, Status? Status, double? Milliseconds, string MutationChain, string? Error);