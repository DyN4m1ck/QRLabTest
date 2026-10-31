namespace QRLab.Core.Models;

/// Устаревшее описание шага мутации: только имя и «сила», без параметров
/// и изображений. Реальные цепочки искажений хранятся как MutationFrame.
public sealed class MutationStep
{
    /// Уникальный идентификатор шага.
    public Guid Id { get; init; } = Guid.NewGuid();
    /// Название операции для отображения.
    public string Name { get; set; } = string.Empty;
    /// Интенсивность эффекта в произвольных единицах (0..1 у UI-слайдеров).
    public double Intensity { get; set; }
}
