using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.View.Presentation;
using QRLab.View.Presentation.Mutations;

namespace QRLab.View.ViewModels;

public sealed partial class MutationsViewModel : ScreenViewModel
{
    private bool _loading;
    private double _centerX = MutationDefaults.CenterX, _centerY = MutationDefaults.CenterY;
    private readonly Stack<OperationViewModel> _undone = new();
    public WorkspaceViewModel Workspace { get; }
    public IReadOnlyList<EnumOption<MutationTool>> Tools => MutationChoices.Tools;
    public IReadOnlyList<EnumOption<MutationMode>> Modes => MutationChoices.Modes;
    public IReadOnlyList<EnumOption<ImageInterpolation>> Interpolations => MutationChoices.Interpolations;
    [ObservableProperty] private MutationTool _tool = MutationTool.Brush;
    [ObservableProperty] private MutationMode _mode = MutationMode.Manual;
    [ObservableProperty] private double _brushSize = MutationDefaults.BrushSize;
    [ObservableProperty] private double _opacity = MutationDefaults.Opacity;
    [ObservableProperty] private double _angleX = MutationDefaults.AngleX;
    [ObservableProperty] private double _angleY = MutationDefaults.AngleY;
    [ObservableProperty] private double _blur = MutationDefaults.Sigma;
    [ObservableProperty] private decimal? _seed = MutationDefaults.Seed;
    [ObservableProperty] private ImageInterpolation _interpolation = ImageInterpolation.Bicubic;
    [ObservableProperty] private OperationViewModel? _selectedOperation;
    [ObservableProperty] private bool _isBusy;
    public ObservableCollection<OperationViewModel> Operations => Workspace.SelectedSample?.Operations ?? [];
    public IReadOnlyList<OperationViewModel> Frames => Workspace.SelectedSample is { } sample
        ? new[] { sample.OriginalFrame }.Concat(Operations).ToArray() : [];
    public OperationViewModel? SelectedFrame
    {
        get => SelectedOperation ?? Workspace.SelectedSample?.OriginalFrame;
        set => SelectedOperation = value?.Options is null ? null : value;
    }
    public RasterImage? PreviewImage => SelectedOperation?.Image ?? Workspace.SelectedSample?.OriginalImage;
    public bool IsManual => Mode == MutationMode.Manual;
    public bool IsHand => Tool == MutationTool.Hand;
    public bool IsBlur => SelectedOperation?.Options is GaussianBlurOptions;
    public bool IsInk => SelectedOperation?.Options is InkMutationOptions;
    public bool IsPerspective => SelectedOperation?.Options is PerspectiveMutationOptions;
    public bool CanUndo => Operations.Count > 0;
    public bool CanRedo => _undone.Count > 0;

    public MutationsViewModel(WorkspaceViewModel workspace) : base("02", "Мутации", "Редактор искажений / цепочка операций")
    {
        Workspace = workspace;
        _selectedOperation = Operations.LastOrDefault();
        Workspace.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(Workspace.SelectedSample)) return;
            _undone.Clear();
            Refresh();
            SelectedOperation = Operations.LastOrDefault();
            OnPropertyChanged(nameof(SelectedFrame));
        };
    }

    partial void OnModeChanged(MutationMode value) => OnPropertyChanged(nameof(IsManual));
    partial void OnToolChanged(MutationTool value) => OnPropertyChanged(nameof(IsHand));
    partial void OnAngleXChanged(double value) => UpdatePreview();
    partial void OnAngleYChanged(double value) => UpdatePreview();
    partial void OnBlurChanged(double value) => UpdatePreview();
    partial void OnBrushSizeChanged(double value) => UpdatePreview();
    partial void OnInterpolationChanged(ImageInterpolation value) => UpdatePreview();
    partial void OnOpacityChanged(double value) => UpdatePreview();

    private MutationOptions CurrentOptions(MutationKind kind) => kind switch
    {
        MutationKind.GaussianBlur => new GaussianBlurOptions { Sigma = Blur },
        MutationKind.Ink => new InkMutationOptions
        {
            BrushSize = BrushSize, Opacity = Opacity, CenterX = _centerX, CenterY = _centerY,
            Erase = Tool == MutationTool.Eraser, Seed = (int)(Seed ?? MutationDefaults.Seed)
        },
        MutationKind.Perspective => new PerspectiveMutationOptions
        {
            AngleX = AngleX, AngleY = AngleY, Interpolation = Interpolation, Seed = (int)(Seed ?? MutationDefaults.Seed)
        },
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private void UpdatePreview()
    {
        if (_loading || SelectedOperation?.Options is not { } options) return;
        var index = Operations.IndexOf(SelectedOperation);
        if (index < 0 || Workspace.SelectedSample is not { } sample) return;
        var chain = Operations.Select(o => o.Options!).ToArray();
        chain[index] = CurrentOptions(options.Kind);
        try
        {
            var frames = Workspace.Mutate(sample.OriginalImage, chain);
            _loading = true;
            try
            {
                ReplaceOperations(sample, frames);
                _undone.Clear();
                Refresh();
                SelectedOperation = sample.Operations[index];
                OnPropertyChanged(nameof(SelectedFrame));
                OnPropertyChanged(nameof(PreviewImage));
            }
            finally { _loading = false; }
        }
        catch (ArgumentException exception) { Workspace.ShowError(exception.Message); }
    }

    partial void OnSelectedOperationChanged(OperationViewModel? value)
    {
        if (_loading) return;
        _loading = true;
        switch (value?.Options)
        {
            case GaussianBlurOptions blur: Blur = blur.Sigma; break;
            case InkMutationOptions ink:
                BrushSize = ink.BrushSize; Opacity = ink.Opacity; _centerX = ink.CenterX; _centerY = ink.CenterY; Seed = ink.Seed; break;
            case PerspectiveMutationOptions perspective:
                AngleX = perspective.AngleX; AngleY = perspective.AngleY; Seed = perspective.Seed; Interpolation = perspective.Interpolation; break;
        }
        _loading = false;
        foreach (var name in new[] { nameof(SelectedFrame), nameof(PreviewImage), nameof(IsBlur), nameof(IsInk), nameof(IsPerspective) })
            OnPropertyChanged(name);
    }

    [RelayCommand]
    private void Paint(PreviewPoint point)
    {
        if (!IsManual || IsHand) return;
        _centerX = point.X; _centerY = point.Y; Add(MutationKind.Ink);
    }

    [RelayCommand]
    private void Add(MutationKind kind)
    {
        if (Workspace.SelectedSample is not { } sample) return;
        try
        {
            var options = CurrentOptions(kind);
            var frame = Workspace.Mutate(sample.Image, [options]).Single();
            Operations.Add(new($"{Operations.Count + 1:00}", frame.Options, frame.Image));
            _undone.Clear(); Refresh();
            SelectedOperation = Operations.Last();
        }
        catch (ArgumentException exception) { Workspace.ShowError(exception.Message); }
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        _undone.Push(Operations[^1]); Operations.RemoveAt(Operations.Count - 1);
        Refresh(); SelectedOperation = Operations.LastOrDefault();
    }

    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        Operations.Add(_undone.Pop()); Refresh(); SelectedOperation = Operations.Last();
    }

    [RelayCommand]
    private void Reset()
    {
        Operations.Clear(); _undone.Clear(); Refresh(); SelectedOperation = null;
    }

    [RelayCommand]
    private void Randomize()
    {
        Seed = (Seed ?? MutationDefaults.Seed) + 1;
        Add(MutationKind.Perspective);
    }

    [RelayCommand]
    private async Task Apply()
    {
        var chain = Operations.Select(o => o.Options!).ToArray();
        var targets = Workspace.Samples.Where(s => s != Workspace.SelectedSample).ToArray();
        IsBusy = true;
        try
        {
            var results = await Task.Run(() => targets.Select(sample =>
                (Sample: sample, Frames: Workspace.Mutate(sample.OriginalImage, chain))).ToArray());
            foreach (var result in results) ReplaceOperations(result.Sample, result.Frames);
            Refresh();
            Workspace.Notice = $"Цепочка из {chain.Length} операций назначена {targets.Length + 1} образцам.";
        }
        catch (ArgumentException exception) { Workspace.ShowError(exception.Message); }
        finally { IsBusy = false; }
    }

    private static void ReplaceOperations(SampleViewModel sample, IReadOnlyList<MutationFrame> frames)
    {
        sample.Operations.Clear();
        foreach (var (frame, index) in frames.Select((frame, index) => (frame, index)))
            sample.Operations.Add(new($"{index + 1:00}", frame.Options, frame.Image));
    }

    private void Refresh()
    {
        foreach (var name in new[] { nameof(Operations), nameof(Frames), nameof(SelectedFrame), nameof(PreviewImage), nameof(CanUndo), nameof(CanRedo) })
            OnPropertyChanged(name);
        UndoCommand.NotifyCanExecuteChanged(); RedoCommand.NotifyCanExecuteChanged();
    }
}
