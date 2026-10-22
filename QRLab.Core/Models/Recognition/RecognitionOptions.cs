namespace QRLab.Core.Models.Recognition;

public sealed record RecognitionOptions(string Decoder = "ZXing", int Timeout = 500, bool Enhanced = true);
