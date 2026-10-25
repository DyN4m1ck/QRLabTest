namespace QRLab.Core.Models.Mutations;

public sealed record SaltAndPepperOptions : MutationOptions
{
    public override MutationKind Kind => MutationKind.SaltAndPepper;
    public double DensityPercent { get; init; } = MutationDefaults.NoisePercent;
    public double SaltPercent { get; init; } = MutationDefaults.SaltPercent;
    public int Seed { get; init; } = MutationDefaults.Seed;

    public override void Validate()
    {
        if (!double.IsFinite(DensityPercent) || DensityPercent is < 0 or > 100
            || !double.IsFinite(SaltPercent) || SaltPercent is < 0 or > 100)
            throw new ArgumentException("Плотность шума и доля белых точек должны быть от 0 до 100%.");
    }
}