using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRLab.Core.Interfaces.Generation;
using QRLab.Core.Interfaces.Mutations;
using QRLab.Core.Interfaces.Recognition;
using QRLab.Core.Interfaces.Storage;
using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Generation.Qrcode;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;
using QRLab.Core.Models.Storage;

namespace QRLab.View.ViewModels.Workspace;

public sealed partial class WorkspaceViewModel : ObservableObject
{
    private readonly ICodeGenerator _generator;
    private readonly ISampleSetStorage _storage;
    private readonly IMutationProcessor _mutations;
    private readonly ICodeRecognizer _recognizer;
    
    public ObservableCollection<Samples.SampleViewModel> Samples { get; } = [];

    [ObservableProperty] private Samples.SampleViewModel? _selectedSample;
    [ObservableProperty] private string _notice = "";
    [ObservableProperty] private string _setName = GenerationDefaults.NewSetName;
    [ObservableProperty] private string _error = "";

    public bool HasError => !string.IsNullOrEmpty(Error);
    public string Position => $"{(SelectedSample is null ? 0 : Samples.IndexOf(SelectedSample) + 1)} / {Samples.Count}";
    public int SampleCount => Samples.Count;
    public decimal SampleNumberMaximum => Math.Max(1, Samples.Count);
    public decimal? SampleNumber
    {
        get => SelectedSample is null ? null : Samples.IndexOf(SelectedSample) + 1;
        set
        {
            if (value is null || value != decimal.Truncate(value.Value) || value < 1 || value > Samples.Count) return;
            SelectedSample = Samples[(int)value.Value - 1];
        }
    }
    public bool CanMovePrevious => SelectedSample is not null && Samples.IndexOf(SelectedSample) > 0;
    public bool CanMoveNext => SelectedSample is not null && Samples.IndexOf(SelectedSample) < Samples.Count - 1;
    public IReadOnlyList<LibrarySet> LibrarySets => GenerationPresets.Library;

    public WorkspaceViewModel(ICodeGenerator generator, ISampleSetStorage storage, IMutationProcessor mutations, ICodeRecognizer recognizer)
    {
        _generator = generator;
        _storage = storage;
        _mutations = mutations;
        _recognizer = recognizer;
        Generate(new QrGenerationOptions());
        Notice = $"Подготовлено {Samples.Count} образцов";
    }

    partial void OnErrorChanged(string value) => OnPropertyChanged(nameof(HasError));
    partial void OnSelectedSampleChanged(Samples.SampleViewModel? value)
    {
        OnPropertyChanged(nameof(Position));
        OnPropertyChanged(nameof(SampleNumber));
        PreviousCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    public bool Generate(GenerationOptions options)
    {
        try
        {
            ReplaceSamples(_generator.CreateSamples(options));
            SetName = GenerationDefaults.NewSetName;
            Notice = $"Подготовлено {Samples.Count} образцов";
            Error = "";
            return true;
        }
        catch (ArgumentException exception) { ShowError(exception.Message); return false; }
    }

    private void ReplaceSamples(IReadOnlyList<SamplePreview> samples)
    {
        var models = samples.Select(s => new Samples.SampleViewModel(s)).ToArray();
        Samples.Clear();
        foreach (var sample in models) Samples.Add(sample);
        SelectedSample = Samples.FirstOrDefault();
        OnPropertyChanged(nameof(Position));
        OnPropertyChanged(nameof(SampleCount));
        OnPropertyChanged(nameof(SampleNumberMaximum));
    }

    public void ShowError(string message) { Error = message; Notice = message; }
    public IReadOnlyList<MutationFrame> Mutate(RasterImage original, IReadOnlyList<MutationOptions> operations) =>
        _mutations.Apply(original, operations);

    public async Task<bool> ExportAsync(ExportOptions options)
    {
        var samples = Samples.Select(s => s.Preview).ToArray();
        try
        {
            var set = await Task.Run(() => _storage.Save(samples, options));
            SetName = set.Name;
            Notice = $"Сохранён набор «{SetName}»: {samples.Length} образцов.";
            Error = "";
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException)
        { ShowError(exception.Message); return false; }
    }

    public SampleSet? LoadFolder(string directory)
    {
        try
        {
            var set = _storage.Load(directory);
            ReplaceSamples(set.Samples);
            SetName = set.Name;
            Notice = $"Загружен набор «{SetName}»: {Samples.Count} образцов";
            Error = "";
            return set;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidDataException or UnauthorizedAccessException)
        { ShowError(exception.Message); return null; }
    }

    public async Task RecognizeAsync(RecognitionOptions options, CancellationToken cancellationToken = default)
    {
        var snapshot = Samples.Select(sample => (Model: sample, Image: sample.Image, Preview: sample.Preview)).ToArray();
        try
        {
            var results = await _recognizer.RecognizeAsync(snapshot.Select(item => item.Preview).ToArray(), options, cancellationToken);
            foreach (var result in results)
            {
                var item = snapshot.First(item => item.Preview.Number == result.Number);
                if (Samples.Contains(item.Model) && ReferenceEquals(item.Model.Image, item.Image)) item.Model.Result = result;
            }
            Error = "";
            Notice = $"Обработано {results.Count} образцов";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        { Notice = "Распознавание отменено."; }
    }

    [RelayCommand(CanExecute = nameof(CanMovePrevious))] private void Previous() => Move(-1);
    [RelayCommand(CanExecute = nameof(CanMoveNext))] private void Next() => Move(1);
    private void Move(int delta)
    {
        if (Samples.Count > 0)
            SelectedSample = Samples[Math.Clamp(Samples.IndexOf(SelectedSample!) + delta, 0, Samples.Count - 1)];
    }
}
