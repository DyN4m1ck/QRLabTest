namespace QRLab.Core.Models;

/// <summary>
/// Устаревшее описание шага мутации: только имя и «сила», без параметров
/// и изображений. Реальные цепочки искажений хранятся как MutationFrame.
/// </summary>
public sealed class MutationStep
{
    /// <summary>Уникальный идентификатор шага.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>Название операции для отображения.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Интенсивность эффекта в произвольных единицах (0..1 у UI-слайдеров).</summary>
    public double Intensity { get; set; }
}
