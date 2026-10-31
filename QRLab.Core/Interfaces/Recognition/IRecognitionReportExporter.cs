using QRLab.Core.Models.Recognition;

namespace QRLab.Core.Interfaces.Recognition;

/// Экспортёр отчёта о распознавании в файл (в текущей реализации — CSV).
/// Вынесен в интерфейс, чтобы формат отчёта можно было менять, не трогая ViewModel.
public interface IRecognitionReportExporter
{
    /// Сохраняет строки отчёта по указанному пути. Операция асинхронная и отменяемая:
    /// запись выполняется во временный файл с последующей атомарной заменой,
    /// поэтому отмена не оставляет «полусозданный» отчёт.
    Task ExportAsync(string path, IReadOnlyList<RecognitionReportRow> rows, CancellationToken cancellationToken = default);
}
