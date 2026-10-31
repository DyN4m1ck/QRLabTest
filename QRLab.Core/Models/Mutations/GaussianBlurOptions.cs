namespace QRLab.Core.Models.Mutations;

/// <summary>Параметры гауссова размытия: одна величина sigma (в пикселях).</summary>
public sealed record GaussianBlurOptions : MutationOptions
{
    /// <summary>Вид мутации — размытие.</summary>
    public override MutationKind Kind => MutationKind.GaussianBlur;

    /// <summary>Стандартное отклонение гауссова ядра; 0 означает «без эффекта».</summary>
    public double Sigma { get; init; } = MutationDefaults.Sigma;

    /// <summary>Проверяет конечность и диапазон sigma.</summary>
    public override void Validate()
    {
        if (!double.IsFinite(Sigma) || Sigma < MutationDefaults.MinimumSigma || Sigma > MutationDefaults.MaximumSigma)
            throw new ArgumentException($"Сила размытия должна быть от 0 до {MutationDefaults.MaximumSigma}.");
    }
}
