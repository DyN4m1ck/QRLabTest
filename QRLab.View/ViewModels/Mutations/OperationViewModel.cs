using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.View.Presentation;
using QRLab.View.Presentation.Mutations;

namespace QRLab.View.ViewModels;

public sealed record OperationViewModel(string Number, MutationOptions? Options, RasterImage Image)
{
    public string Name => Options is null ? MutationChoices.Original.Name
        : GenerationChoices.Name(MutationChoices.Operations, Options.Kind);
    public string Details => Options is null ? MutationChoices.Original.Description : MutationChoices.Describe(Options);
}