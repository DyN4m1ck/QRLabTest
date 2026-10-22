using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Mutations;

/// <summary>Reserved implementation. Preserves the exact source until ink processing is added.</summary>
public sealed class InkMutation : ImageMutation<InkMutationOptions>
{
    public override MutationKind Kind => MutationKind.Ink;
    protected override RasterImage Apply(RasterImage image, InkMutationOptions options) => image;
}