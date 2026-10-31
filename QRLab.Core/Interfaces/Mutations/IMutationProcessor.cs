using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Core.Interfaces.Mutations;

/// Последовательный конвейер мутаций: применяет список операций одну за другой.
/// Результат каждой операции сохраняется отдельным кадром, чтобы интерфейс мог
/// показывать весь ход искажений, а не только финальную картинку.
public interface IMutationProcessor
{
    /// Прогоняет изображение через цепочку операций и возвращает кадры
    /// (параметры + полученное после них изображение) в порядке применения.
    IReadOnlyList<MutationFrame> Apply(RasterImage original, IReadOnlyList<MutationOptions> operations);
}
