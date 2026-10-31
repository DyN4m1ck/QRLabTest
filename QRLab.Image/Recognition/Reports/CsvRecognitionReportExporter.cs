using System.Globalization;
using System.Text;
using QRLab.Core.Interfaces.Recognition;
using QRLab.Core.Models.Recognition;

namespace QRLab.Image.Recognition.Reports;

// Экспорт результатов распознавания в CSV (разделитель «;» — дружит с русской локали Excel).
// Все значения экранируются кавычками, файл пишется через временный «черновик» и атомарно
// перемещается на место, чтобы при сбое пользователь не получил полузаписанный отчёт.
public sealed class CsvRecognitionReportExporter : IRecognitionReportExporter
{
    // Декларативное описание отчёта: номер, образец, формат, ожидания/факт, статус,
    // время в мс (F3 — три знака), цепочка мутаций и текст ошибки. Числа форматируются
    // через InvariantCulture, чтобы десятичный разделитель был стабильным.
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
        // Скрытый временный файл в той же папке: File.Move поверх целевого пути атомарен
        // в пределах одного тома, поэтому готовый CSV никогда не бывает «неполным».
        var temporary = Path.Combine(directory, $".qrlab-csv-{Guid.NewGuid():N}");
        try
        {
            // UTF-8 с BOM — требование Excel: без маркера кириллические заголовки отображаются кракозябрами.
            await using (var writer = new StreamWriter(temporary, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            {
                await writer.WriteLineAsync(string.Join(';', Columns.Select(column => Escape(column.Name))).AsMemory(), cancellationToken);
                foreach (var row in rows)
                    await writer.WriteLineAsync(string.Join(';', Columns.Select(column => Escape(column.Value(row)))).AsMemory(), cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            // Финальная публикация файла; отмена после этой точки уже не повредит записи.
            File.Move(temporary, path, overwrite: true);
        }
        // Гарантированная очистка черновика при любом исходе (включая отмену и исключение).
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static string Escape(string? value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
}