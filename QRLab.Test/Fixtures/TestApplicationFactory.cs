using QRLab.Image.Generation;
using QRLab.Image.Mutations;
using QRLab.Image.Recognition;
using QRLab.Image.Storage;
using QRLab.View.ViewModels;

namespace QRLab.Test.Fixtures;

internal static class TestApplicationFactory
{
    public static SampleSetStorage CreateStorage() => new();
    public static MutationProcessor CreateMutations() => new([new GaussianBlurMutation(), new InkMutation(), new PerspectiveMutation()]);
    public static MainWindowViewModel CreateMainWindow() => new(new CodeGenerator(), CreateStorage(), CreateMutations(), new DemoCodeRecognizer());
    public static WorkspaceViewModel CreateWorkspace() => new(new CodeGenerator(), CreateStorage(), CreateMutations(), new DemoCodeRecognizer());
}