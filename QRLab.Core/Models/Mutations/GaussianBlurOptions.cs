namespace QRLab.Core.Models.Mutations;

public sealed record GaussianBlurOptions : MutationOptions
{
    public override MutationKind Kind => MutationKind.GaussianBlur;

    public double Sigma { get; init; } = MutationDefaults.Sigma;

    public override void Validate()
    {
        if (!double.IsFinite(Sigma) || MutationDefaults.MinimumSigma < 0 || Sigma > MutationDefaults.MaximumSigma)
            throw new ArgumentException($"Сила размытия должна быть от 0 до {MutationDefaults.MaximumSigma}.");
    }
}