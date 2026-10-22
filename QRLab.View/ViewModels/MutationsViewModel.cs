using QRLab.Core.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace QRLab.View.ViewModels;
public sealed partial class MutationsViewModel : ScreenViewModel
{
    private bool _loading;
    private double _centerX = .62, _centerY = .48;
    private readonly Stack<OperationViewModel> _undone = new();
    public WorkspaceViewModel Workspace { get; }
    public IReadOnlyList<string> Tools { get; } = ["Кисть", "Ластик", "Рука"];
    public IReadOnlyList<string> Modes { get; } = ["Вручную", "Алгоритм"];
    public IReadOnlyList<string> Interpolations { get; } = ["Бикубическая", "Билинейная", "Ближайший сосед"];
    [ObservableProperty] private string _tool = "Кисть";
    [ObservableProperty] private string _mode = "Вручную";
    [ObservableProperty] private double _brushSize = 48;
    [ObservableProperty] private double _opacity = 100;
    [ObservableProperty] private double _angleX = 8;
    [ObservableProperty] private double _angleY = -4;
    [ObservableProperty] private double _blur = 1.2;
    [ObservableProperty] private decimal? _seed = 42;
    [ObservableProperty] private string _interpolation = "Бикубическая";
    [ObservableProperty] private OperationViewModel? _selectedOperation;
    public ObservableCollection<OperationViewModel> Operations => Workspace.SelectedSample?.Operations ?? [];
    public IReadOnlyList<OperationViewModel> Frames => new[] { new OperationViewModel("00", "Исходник", "Защищён", 0, Workspace.SelectedSample?.Modules) }.Concat(Operations).ToArray();
    public bool IsManual => Mode == "Вручную";
    public bool IsHand => Tool == "Рука";
    partial void OnModeChanged(string value) => OnPropertyChanged(nameof(IsManual));
    partial void OnToolChanged(string value) => OnPropertyChanged(nameof(IsHand));
    [RelayCommand] private void Paint(PreviewPoint point)
    {
        if (!IsManual || IsHand) return;
        _centerX = point.X; _centerY = point.Y; Add("2");
    }
    public int Stage => SelectedOperation?.Stage ?? 0;
    public bool[,]? PreviewModules => Workspace.Frame(Workspace.SelectedSample, CurrentOptions(Stage));
    partial void OnAngleXChanged(double value) => UpdatePreview();
    partial void OnAngleYChanged(double value) => UpdatePreview();
    partial void OnBlurChanged(double value) => UpdatePreview();
    partial void OnBrushSizeChanged(double value) => UpdatePreview();
    partial void OnInterpolationChanged(string value) => UpdatePreview();
    partial void OnOpacityChanged(double value) => UpdatePreview();
    private FrameOptions CurrentOptions(int stage) => new(stage, AngleX, AngleY, Blur, BrushSize, Opacity, _centerX, _centerY, Tool == "Ластик", (int)(Seed ?? 42), Interpolation);
    private string Describe(int stage) => stage switch { 1 => $"σ = {Blur:N1} px", 2 => $"Кисть {BrushSize:N0} px • {Opacity:N0}%", _ => $"X: {AngleX:N0}° • Y: {AngleY:N0}°" };
    private void UpdatePreview()
    {
        if (_loading) return;
        if (SelectedOperation is { Stage: > 0 } operation && Operations.IndexOf(operation) is var index && index >= 0)
        {
            var updated = operation with { Options = CurrentOptions(Stage), Details = Describe(Stage), Modules = PreviewModules };
            _loading = true; Operations[index] = updated; SelectedOperation = updated; _loading = false;
            OnPropertyChanged(nameof(Frames));
        }
        OnPropertyChanged(nameof(PreviewModules));
    }
    public bool IsBlur => Stage == 1;
    public bool IsInk => Stage == 2;
    public bool IsPerspective => Stage == 3;
    public bool CanUndo => Operations.Count > 0;
    public bool CanRedo => _undone.Count > 0;
    public MutationsViewModel(WorkspaceViewModel workspace) : base("02", "Мутации", "Редактор искажений / цепочка операций")
    {
        Workspace = workspace; _selectedOperation = Operations.LastOrDefault();
        Workspace.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(Workspace.SelectedSample)) return;
            _undone.Clear(); SelectedOperation = Operations.LastOrDefault(); Refresh();
        };
    }
    partial void OnSelectedOperationChanged(OperationViewModel? value)
    {
        if (!_loading && value?.Options is { } options)
        {
            _loading = true;
            AngleX = options.AngleX;AngleY = options.AngleY; Blur = options.Blur;
            BrushSize = options.BrushSize; Opacity = options.Opacity;
            _centerX = options.CenterX; _centerY = options.CenterY;
            Seed = options.Seed; Interpolation = options.Interpolation;
            _loading = false;
        }
        foreach (var name in new[] { nameof(PreviewModules), nameof(Stage), nameof(IsBlur), nameof(IsInk), nameof(IsPerspective) }) OnPropertyChanged(name);
    }
    [RelayCommand(CanExecute = nameof(CanUndo))] private void Undo()
    {
        _undone.Push(Operations[^1]); Operations.RemoveAt(Operations.Count - 1); SelectedOperation = Operations.LastOrDefault(); Refresh();
    }
    [RelayCommand(CanExecute = nameof(CanRedo))] private void Redo()
    {
        Operations.Add(_undone.Pop()); SelectedOperation = Operations.Last(); Refresh();
    }
    [RelayCommand] private void Add(string kind)
    {
        var stage = int.Parse(kind);
        var name = stage switch { 1 => "Размытие Гаусса", 2 => "Кляксы", _ => "Перспектива" };
        var details = stage switch { 1 => $"σ = {Blur:N1} px", 2 => $"Кисть {BrushSize:N0} px • {Opacity:N0}%", _ => $"X: {AngleX:N0}° • Y: {AngleY:N0}°" };
        var operation = new OperationViewModel($"{Operations.Count + 1:00}", name, details, stage, Workspace.Frame(Workspace.SelectedSample, CurrentOptions(stage)), CurrentOptions(stage));
        Operations.Add(operation); SelectedOperation = operation; _undone.Clear(); Refresh();
    }
    [RelayCommand] private void Reset() { Operations.Clear(); _undone.Clear(); SelectedOperation = null; Refresh(); }
    [RelayCommand] private void Randomize()
    {
        Seed = (Seed ?? 42) + 1;
        var variant = Workspace.CreateVariant((int)Seed, CurrentOptions(3));
        AngleX = variant.AngleX; AngleY = variant.AngleY; Add("3");
    }
    [RelayCommand] private void Apply()
    {
        foreach (var sample in Workspace.Samples.Where(s => s != Workspace.SelectedSample))
        {
            sample.Operations.Clear(); foreach (var operation in Operations) sample.Operations.Add(operation with { Modules = Workspace.Frame(sample, operation.Options ?? new(operation.Stage)) });
        }
        Workspace.Notice = $"Цепочка из {Operations.Count} операций назначена {Workspace.Samples.Count} образцам (макет).";
    }
    private void Refresh()
    {
        OnPropertyChanged(nameof(Operations)); OnPropertyChanged(nameof(Frames));
        OnPropertyChanged(nameof(CanUndo)); OnPropertyChanged(nameof(CanRedo));
        UndoCommand.NotifyCanExecuteChanged(); RedoCommand.NotifyCanExecuteChanged();
    }
}
