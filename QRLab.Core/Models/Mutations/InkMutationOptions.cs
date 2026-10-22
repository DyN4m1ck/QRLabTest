namespace QRLab.Core.Models.Mutations;

public sealed record InkMutationOptions : MutationOptions
{
    public double BrushSize { get; init; } = MutationDefaults.BrushSize;
    public double Opacity { get; init; } = MutationDefaults.Opacity;
    public double CenterX { get; init; } = MutationDefaults.CenterX;
    public double CenterY { get; init; } = MutationDefaults.CenterY;
    public bool Erase { get; init; }
    public int Seed { get; init; } = MutationDefaults.Seed;
    public override MutationKind Kind => MutationKind.Ink;

    public override void Validate()
    {
        if (!double.IsFinite(BrushSize) || BrushSize <= 0 || !double.IsFinite(Opacity) || Opacity is < 0 or > 100
            || !double.IsFinite(CenterX) || CenterX is < 0 or > 1 || !double.IsFinite(CenterY) || CenterY is < 0 or > 1)
            throw new ArgumentException("Недопустимые параметры кисти.");
    }
}