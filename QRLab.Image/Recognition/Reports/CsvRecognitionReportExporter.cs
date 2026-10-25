using System.Globalization;
using System.Text;
using QRLab.Core.Interfaces.Recognition;
using QRLab.Core.Models.Recognition;

namespace QRLab.Image.Recognition.Reports;

public sealed class CsvRecognitionReportExporter : IRecognitionReportExporter
{
    private static readonly CsvColumn[] Columns =
    [
        new("Номер", row => row.Number.ToString(CultureInfo.InvariantCulture)),
        new("Образец", row => row.Name),
        new("Формат", row => row.Format),
        new("Исходное содержимое", row => row.ExpectedContent),
        new("Считанное содержимое", row => row.DecodedContent),
        new("Результат", row => row.Status?.ToString()),
        new("Время, мс", row => row.Milliseconds?.ToString("F3", CultureInfo.InvariantCulture)),
        new("Цепочка мутаций", row => row.MutationChain),
        new("Ошибка", row => row.Error)
    ];

    public async Task ExportAsync(string path, IReadOnlyList<RecognitionReportRow> rows, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Укажите путь к CSV-файлу.");
        path = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);
        var temporary = Path.Combine(directory, $".qrlab-csv-{Guid.NewGuid():N}");
        try
        {
            await using (var writer = new StreamWriter(temporary, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            {
                await writer.WriteLineAsync(string.Join(';', Columns.Select(column => Escape(column.Name))).AsMemory(), cancellationToken);
                foreach (var row in rows)
                    await writer.WriteLineAsync(string.Join(';', Columns.Select(column => Escape(column.Value(row)))).AsMemory(), cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static string Escape(string? value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}