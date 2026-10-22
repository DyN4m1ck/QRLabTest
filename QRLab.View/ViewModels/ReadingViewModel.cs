using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRLab.Core.Interfaces;
using QRLab.Core.Models.Recognition;

namespace QRLab.View.ViewModels;
public sealed partial class ReadingViewModel : ScreenViewModel
{
    public WorkspaceViewModel Workspace { get; }
    public IReadOnlyList<string> Filters { get; } = ["Все", "Успешные", "Ошибки"];
    public IReadOnlyList<string> Decoders { get; } = ["ZXing • 2.3.0", "OpenCV • QRCodeDetector"];
    
    [ObservableProperty] private string _filter = "Все";
    [ObservableProperty] private string _search = "";
    [ObservableProperty] private string _decoder = "ZXing • 2.3.0";
    [ObservableProperty] private decimal? _timeout = 500;
    [ObservableProperty] private bool _enhanced = true;
    [ObservableProperty] private bool _isExportOpen;
    [ObservableProperty] private string _exportDestination = "QRLab / Результаты";
    [ObservableProperty] private SampleViewModel? _selectedResult;
    partial void OnSelectedResultChanged(SampleViewModel? value) { if (value is not null) Workspace.SelectedSample = value; }
    public IReadOnlyList<SampleViewModel> Results => Workspace.Samples.Where(s =>
        (Filter == "Все" || Filter == "Успешные" && s.Result is { Status: Status.Success } || Filter == "Ошибки" && s.IsFailure) &&
        (s.Name.Contains(Search, StringComparison.OrdinalIgnoreCase) || s.Content.Contains(Search, StringComparison.OrdinalIgnoreCase))).ToArray();
    
    public bool NoResults => Results.Count == 0;
    public int ResultsChecked => Workspace.Samples.Count(s => s.Result is not null);
    public int ResultsSuccess => Workspace.Samples.Count(s => s.Result?.Status == Status.Success );
    public int ResultsFailed => Workspace.Samples.Count(s => s.Result?.Status == Status.Failed);
    public int ResultsTimeout => Workspace.Samples.Count(s => s.Result?.Status == Status.Timeout);
    
    public string Average => ResultsChecked == 0 ? "—" : $"{Workspace.Samples.Where(s => s.Result is not null).Average(s => s.Result!.Milliseconds):N1} мс";
    public string Rate => ResultsChecked == 0 ? "Нет результатов" : $"{100d * ResultsSuccess / ResultsChecked:N1}% от проверенных";
    public ReadingViewModel(WorkspaceViewModel workspace) : base("03", "Считывание и статистика", "Набор 01 / Результаты распознавания")
    {
        Workspace = workspace; _selectedResult = workspace.SelectedSample;
        Workspace.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(Workspace.SelectedSample) && Workspace.SelectedSample is not null) SelectedResult = Workspace.SelectedSample; };
        Workspace.Samples.CollectionChanged += (_, _) => Refresh();
    }
    partial void OnFilterChanged(string value) { OnPropertyChanged(nameof(Results)); OnPropertyChanged(nameof(NoResults)); }
    partial void OnSearchChanged(string value) { OnPropertyChanged(nameof(Results)); OnPropertyChanged(nameof(NoResults)); }
    [RelayCommand] private void Run() { Workspace.Recognize(new(Decoder, (int)(Timeout ?? 500), Enhanced)); Refresh(); }
    [RelayCommand] private void ConfirmExport() { Workspace.Notice = "Экспорт результатов распознавания пока не реализован."; IsExportOpen = false; }
    [RelayCommand] private void Export() => IsExportOpen = !IsExportOpen;
    private void Refresh()
    {
        foreach (var name in new[] { nameof(Results), nameof(NoResults), nameof(ResultsChecked), nameof(ResultsSuccess), nameof(ResultsFailed), nameof(Average), nameof(Rate) }) OnPropertyChanged(name);
    }
}
