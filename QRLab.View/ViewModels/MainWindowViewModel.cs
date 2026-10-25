using QRLab.Core.Interfaces.Generation;
using QRLab.Core.Interfaces.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRLab.Core.Interfaces.Mutations;
using QRLab.Core.Interfaces.Recognition;


namespace QRLab.View.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public WorkspaceViewModel Workspace { get; }
    public GenerationViewModel Generation { get; }
    public MutationsViewModel Mutations { get; }
    public ReadingViewModel Reading { get; }
    public IReadOnlyList<ScreenViewModel> Screens { get; }
    [ObservableProperty] private ScreenViewModel _selectedScreen;
    [ObservableProperty] private bool _isAboutOpen;
    public MainWindowViewModel(ICodeGenerator generator, ISampleSetStorage storage, IMutationProcessor mutations, ICodeRecognizer recognizer, IRecognitionReportExporter reportExporter)
    {
        Workspace = new(generator, storage, mutations, recognizer);
        Generation = new(Workspace); 
        Mutations = new(Workspace);
        Reading = new(Workspace, reportExporter);
        Screens = [Generation, Mutations, Reading];
        _selectedScreen = Generation;
    }
    [RelayCommand] private void Navigate(string number) => SelectedScreen = Screens.First(s => s.Number == number);
    [RelayCommand] private void About() => IsAboutOpen = !IsAboutOpen;
}
