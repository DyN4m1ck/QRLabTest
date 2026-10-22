using QRLab.Core.Interfaces.Mutations;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Mutations;

public sealed class MutationProcessor(IEnumerable<IImageMutation> mutations) : IMutationProcessor
{
    private readonly IReadOnlyDictionary<MutationKind, IImageMutation> _mutations = mutations.ToDictionary(m => m.Kind);

    public IReadOnlyList<MutationFrame> Apply(RasterImage original, IReadOnlyList<MutationOptions> operations)
    {
        var frames = new List<MutationFrame>(operations.Count);
        var image = original;
        foreach (var options in operations)
        {
            if (!_mutations.TryGetValue(options.Kind, out var mutation))
                throw new ArgumentException("Реализация мутации не зарегистрирована.");
            image = mutation.Apply(image, options);
            frames.Add(new(options, image));
        }
        return frames;
    }
}