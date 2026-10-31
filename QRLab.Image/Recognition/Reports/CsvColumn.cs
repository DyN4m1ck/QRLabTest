using QRLab.Core.Models.Recognition;

namespace QRLab.Image.Recognition.Reports;

// Описание одной колонки CSV: заголовок + функция извлечения значения из строки отчёта.
// Такой табличный подход позволяет менять состав/порядок колонок в одном месте (см. Columns).
internal sealed record CsvColumn(string Name, Func<RecognitionReportRow, string?> Value);