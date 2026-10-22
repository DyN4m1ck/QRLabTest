using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRLab.Core.Services;
using ResultStatus = QRLab.Core.Services.Status;


namespace QRLab.View.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public WorkspaceViewModel Workspace { get; }
    public GenerationViewModel Generation { get; }
    public MutationsViewModel Mutations { get; }
    public ReadingViewModel Reading { get; }
    public ComparisonViewModel Comparison { get; }
    public IReadOnlyList<ScreenViewModel> Screens { get; }
    [ObservableProperty] private ScreenViewModel _selectedScreen;
    [ObservableProperty] private bool _isAboutOpen;
    public MainWindowViewModel(IPrototypeData data)
    {
        Workspace = new(data);
        Generation = new(Workspace); 
        Mutations = new(Workspace);
        Reading = new(Workspace); 
        Comparison = new(Workspace);
        Screens = [Generation, Mutations, Reading, Comparison]; 
        _selectedScreen = Generation;
    }
    partial void OnSelectedScreenChanged(ScreenViewModel value) { if (value == Comparison) Comparison.Refresh(); }
    [RelayCommand] private void Navigate(string number) => SelectedScreen = Screens.First(s => s.Number == number);
    [RelayCommand] private void About() => IsAboutOpen = !IsAboutOpen;
}
public sealed partial class WorkspaceViewModel : ObservableObject
{
    private readonly IPrototypeData _data;
    public ObservableCollection<SampleViewModel> Samples { get; } = [];
    [ObservableProperty] private SampleViewModel? _selectedSample;
    [ObservableProperty] private string _notice = "Интерактивный макет• демонстрационные данные";
    public string Position => $"{(SelectedSample is null ? 0 : Samples.IndexOf(SelectedSample) + 1)} / {Samples.Count}";
    public WorkspaceViewModel(IPrototypeData data)
    {
        _data = data; Generate(new("LAB-2026-{num}", 1, 120, "QR-код")); SelectedSample = Samples[41]; Recognize();
        Notice = "Интерактивный макет • демонстрационные данные";
    }
    partial void OnSelectedSampleChanged(SampleViewModel? value) => OnPropertyChanged(nameof(Position));
    public void Generate(GenerationOptions options)
    {
        var samples = _data.CreateSamples(options); Samples.Clear();
        foreach (var sample in samples) Samples.Add(new(sample, _data));
        SelectedSample = Samples.FirstOrDefault(); OnPropertyChanged(nameof(Position));
        Notice = $"Подготовлено {Samples.Count} демонстрационных образцов";
    }
    public FrameOptions CreateVariant(int seed, FrameOptions current) => _data.CreateVariant(seed, current);
    public bool[,]? Frame(SampleViewModel? sample, FrameOptions options, bool difference = false) => sample is null ? null : _data.CreateFrame(sample.Modules, options, difference);
    public IReadOnlyList<LibrarySet> LibrarySets => _data.LibrarySets;
    public void Export(ExportOptions options) => Notice = _data.Export(Samples.Select(s => s.Preview).ToArray(), options);
    public void Recognize(RecognitionOptions? options = null)
    {
        foreach (var result in _data.Recognize(Samples.Select(s => s.Preview with { Operations = s.Operations.Select(o => new MutationPreview(o.Number, o.Name, o.Details, o.Stage)).ToArray() }).ToArray(), options ?? new("ZXing • 2.3.0", 500, true)))
            Samples.First(s => s.Preview.Number == result.Number).Result = result;
        Notice = "Интерактивный макет • демонстрационные данные";
    }
    [RelayCommand] private void Previous() => Move(-1);
    [RelayCommand] private void Next() => Move(1);
    private void Move(int delta)
    {
        if (Samples.Count > 0) SelectedSample = Samples[Math.Clamp(Samples.IndexOf(SelectedSample!) + delta, 0, Samples.Count - 1)];
    }
}
public sealed partial class SampleViewModel : ObservableObject
{
    public SamplePreview Preview { get; }
    public SampleViewModel(SamplePreview preview, IPrototypeData data)
    {
        Preview = preview;
        foreach (var op in preview.Operations)
            Operations.Add(new(op.Number, op.Name, op.Details, op.Stage, data.CreateFrame(preview.Modules, new(op.Stage)), new(op.Stage)));
    }
    public string Name => Preview.Name;
    public string Content => Preview.Content;
    public string Dimensions => $"{Preview.Settings.Width} × {Preview.Settings.Height} px";
    public string Correction => Preview.Settings.Correction;
    public string Format => Preview.Format;
    public bool[,] Modules => Preview.Modules;
    public ObservableCollection<OperationViewModel> Operations { get; } = [];
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Status), nameof(IsFailure), nameof(Time), nameof(Chain), nameof(Decoded))]
    private RecognitionPreview? _result;

    public string Status => Result?.Status switch
    {
        null        => "Не обработан",
        ResultStatus.Success     => "Считан",
        ResultStatus.Failed     => "Не считан",
        ResultStatus.Timeout     => "Таймаут",
        _           => "ERROR"
    };
    public bool IsFailure => Result is not { Status: Core.Services.Status.Success };
    public string Time => Result is null ? "—" : $"{Result.Milliseconds:N1} мс";
    public string Chain => Result?.Chain ?? "Ожидает распознавания";
    public string Decoded => Result is not { Status: Core.Services.Status.Success } ? Content : "—";
}
public sealed record OperationViewModel(string Number, string Name, string Details, int Stage, bool[,]? Modules = null, FrameOptions? Options = null);
