namespace QRLab.Core.Models.Mutations;

public sealed record PerspectiveMutationOptions : MutationOptions
{
    public override MutationKind Kind => MutationKind.Perspective;
    public double AngleX { get; init; } = MutationDefaults.AngleX;
    public double AngleY { get; init; } = MutationDefaults.AngleY;
    public double Distance { get; init; } = MutationDefaults.Distance;
    public ImageInterpolation Interpolation { get; init; } = ImageInterpolation.Bicubic;

    public override void Validate()
    {
        if (!double.IsFinite(AngleX) || Math.Abs(AngleX) > MutationDefaults.MaximumAngle
            || !double.IsFinite(AngleY) || Math.Abs(AngleY) > MutationDefaults.MaximumAngle
            || !double.IsFinite(Distance) || Distance < MutationDefaults.MinimumDistance || Distance > MutationDefaults.MaximumDistance
            || !Enum.IsDefined(Interpolation))
            throw new ArgumentException("Недопустимые параметры перспективы.");
    }
}