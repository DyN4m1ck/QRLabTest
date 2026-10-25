namespace QRLab.Core.Models.Mutations;

public enum ImageInterpolation
{
    Bicubic, 
    Bilinear, 
    Nearest
}

public sealed record PerspectiveMutationOptions : MutationOptions
{
    public override MutationKind Kind => MutationKind.Perspective;
    
    public double AngleX { get; init; } = MutationDefaults.AngleX;
    public double AngleY { get; init; } = MutationDefaults.AngleY;
    public int Seed { get; init; } = MutationDefaults.Seed;
    public ImageInterpolation Interpolation { get; init; } = ImageInterpolation.Bicubic;

    public override void Validate()
    {
        if (!double.IsFinite(AngleX) || !double.IsFinite(AngleY) || !Enum.IsDefined(Interpolation))
            throw new ArgumentException("Недопустимые параметры перспективы.");
    }
}