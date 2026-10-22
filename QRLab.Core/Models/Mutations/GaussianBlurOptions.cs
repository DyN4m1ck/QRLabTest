namespace QRLab.Core.Models.Mutations;

public sealed record GaussianBlurOptions : MutationOptions
{
    public double Sigma { get; init; } = MutationDefaults.Sigma;
    public override MutationKind Kind => MutationKind.GaussianBlur;

    public override void Validate()
    {
        if (!double.IsFinite(Sigma) || Sigma < 0 || Sigma > MutationDefaults.MaximumSigma)
            throw new ArgumentException($"Сила размытия должна быть от 0 до {MutationDefaults.MaximumSigma}.");
    }
}