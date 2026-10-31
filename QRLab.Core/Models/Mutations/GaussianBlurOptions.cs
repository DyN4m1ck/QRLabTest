namespace QRLab.Core.Models.Mutations;

/// Параметры гауссова размытия: одна величина sigma (в пикселях).
public sealed record GaussianBlurOptions : MutationOptions
{
    /// Вид мутации — размытие.
    public override MutationKind Kind => MutationKind.GaussianBlur;

    /// Стандартное отклонение гауссова ядра; 0 означает «без эффекта».
    public double Sigma { get; init; } = MutationDefaults.Sigma;

    /// Проверяет конечность и диапазон sigma.
    public override void Validate()
    {
        if (!double.IsFinite(Sigma) || Sigma < MutationDefaults.MinimumSigma || Sigma > MutationDefaults.MaximumSigma)
            throw new ArgumentException($"Сила размытия должна быть от 0 до {MutationDefaults.MaximumSigma}.");
    }
}
