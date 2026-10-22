using QRLab.Core.Interfaces.Mutations;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Mutations;

public abstract class ImageMutation<TOptions> : IImageMutation where TOptions : MutationOptions
{
    public abstract MutationKind Kind { get; }

    public RasterImage Apply(RasterImage image, MutationOptions options)
    {
        if (options is not TOptions typed)
            throw new ArgumentException("Тип параметров не соответствует мутации.", nameof(options));
        typed.Validate();
        return Apply(image, typed);
    }

    protected abstract RasterImage Apply(RasterImage image, TOptions options);
}