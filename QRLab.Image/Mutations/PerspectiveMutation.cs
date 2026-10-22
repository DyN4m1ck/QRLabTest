using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Mutations;

/// <summary>Reserved implementation. Preserves the exact source until perspective processing is added.</summary>
public sealed class PerspectiveMutation : ImageMutation<PerspectiveMutationOptions>
{
    public override MutationKind Kind => MutationKind.Perspective;
    protected override RasterImage Apply(RasterImage image, PerspectiveMutationOptions options) => image;
}