using QRLab.Core.Models.Recognition;

namespace QRLab.Image.Recognition.Reports;

internal sealed record CsvColumn(string Name, Func<RecognitionReportRow, string?> Value);