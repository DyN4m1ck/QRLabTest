using QRLab.Core.Models.Images;

namespace QRLab.Core.Models.Mutations;

public sealed record MutationFrame(MutationOptions Options, RasterImage Image);