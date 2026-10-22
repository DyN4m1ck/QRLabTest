using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Generation.Barcode;
using QRLab.Core.Models.Generation.Qrcode;
using QRLab.Core.Models.Storage;
using QRLab.View.Presentation;
using BarcodeSymbology = QRLab.Core.Models.Generation.BarcodeType;

namespace QRLab.View.ViewModels;

public sealed partial class GenerationViewModel(WorkspaceViewModel workspace)
    : ScreenViewModel("01", "Генерация кодов", "Создание кодов и управление наборами")
{
    public WorkspaceViewModel Workspace { get; } = workspace;
    public IReadOnlyList<EnumOption<GenerationMode>> Modes => GenerationChoices.Modes;
    public IReadOnlyList<EnumOption<CodeKind>> Formats => GenerationChoices.Formats;
    public IReadOnlyList<EnumOption<QrCorrectionLevel>> Corrections => GenerationChoices.Corrections;
    public IReadOnlyList<EnumOption<ContentSource>> Sources => GenerationChoices.Sources;
    public IReadOnlyList<EnumOption<BarcodeSymbology>> BarcodeTypes => GenerationChoices.BarcodeTypes;
    public GenerationInputLimits Limits { get; } = new();

    [ObservableProperty] private GenerationMode _mode = GenerationDefaults.Mode;
    [ObservableProperty] private CodeKind _format = GenerationDefaults.Kind;
    [ObservableProperty] private ContentSource _source = GenerationDefaults.Source;
    [ObservableProperty] private string _template = GenerationDefaults.Template;
    [ObservableProperty] private decimal? _firstNumber = GenerationDefaults.FirstNumber;
    [ObservableProperty] private decimal? _count = GenerationDefaults.Count;
    [ObservableProperty] private decimal? _imageWidth = GenerationDefaults.Width;
    [ObservableProperty] private decimal? _imageHeight = GenerationDefaults.Height;
    [ObservableProperty] private decimal? _quietZone = GenerationDefaults.QuietZone;
    [ObservableProperty] private QrCorrectionLevel _correction = GenerationDefaults.Correction;
    [ObservableProperty] private decimal? _qrVersion = GenerationDefaults.AutomaticQrVersion;
    [ObservableProperty] private BarcodeSymbology _barcodeType = GenerationDefaults.Barcode;
    [ObservableProperty] private bool _isLibraryOpen;
    [ObservableProperty] private bool _isExportOpen;
    [ObservableProperty] private string _exportDestination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "QRLab", GenerationDefaults.NewSetName);
    [ObservableProperty] private string _loadDirectory = "";
    [ObservableProperty] private bool _isBusy;

    public bool IsQr => Format == CodeKind.QrCode;
    public bool IsBarcode => Format == CodeKind.Barcode;
    public bool IsEan13 => IsBarcode && BarcodeType == BarcodeSymbology.Ean13;
    public bool IsBatch => Mode == GenerationMode.Batch;
    public string GenerateLabel => IsBatch ? $"Подготовить {Count} кодов" : "Подготовить код";

    partial void OnFormatChanged(CodeKind value)
    {
        OnPropertyChanged(nameof(IsQr));
        OnPropertyChanged(nameof(IsBarcode));
        OnPropertyChanged(nameof(IsEan13));
    }

    partial void OnBarcodeTypeChanged(BarcodeSymbology value) => OnPropertyChanged(nameof(IsEan13));
    partial void OnModeChanged(GenerationMode value)
    {
        OnPropertyChanged(nameof(IsBatch));
        OnPropertyChanged(nameof(GenerateLabel));
    }
    partial void OnCountChanged(decimal? value) => OnPropertyChanged(nameof(GenerateLabel));

    [RelayCommand]
    private void Generate()
    {
        try { Workspace.Generate(CreateOptions()); }
        catch (Exception exception) when (exception is ArgumentException or OverflowException)
        { Workspace.ShowError(exception.Message); }
    }

    private GenerationOptions CreateOptions()
    {
        GenerationOptions settings = IsQr
            ? new QrGenerationOptions { Version = Integer(QrVersion), Correction = Correction }
            : BarcodeGenerationOptions.Create(BarcodeType);
        return settings with
        {
            Template = Template, FirstNumber = Integer(FirstNumber),
            Count = IsBatch ? Integer(Count) : GenerationDefaults.SingleCount,
            Width = Integer(ImageWidth), Height = Integer(ImageHeight),
            QuietZone = Integer(QuietZone), Source = Source
        };
    }

    private static int Integer(decimal? value)
    {
        if (value is null || value != decimal.Truncate(value.Value))
            throw new ArgumentException("Заполните числовые поля целыми числами.");
        return checked((int)value.Value);
    }

    private void ApplyOptions(GenerationOptions settings, int count)
    {
        Template = settings.Template;
        FirstNumber = settings.FirstNumber;
        Count = count;
        Mode = count == GenerationDefaults.SingleCount ? GenerationMode.Single : GenerationMode.Batch;
        Format = settings.Kind;
        ImageWidth = settings.Width;
        ImageHeight = settings.Height;
        QuietZone = settings.QuietZone;
        Source = settings.Source;
        if (settings is QrGenerationOptions qr)
        {
            QrVersion = qr.Version;
            Correction = qr.Correction;
        }
        else if (settings is BarcodeGenerationOptions barcode) BarcodeType = barcode.BarcodeType;
    }

    [RelayCommand] private void OpenLibrary() => IsLibraryOpen = !IsLibraryOpen;
    [RelayCommand] private void Export() => IsExportOpen = !IsExportOpen;

    [RelayCommand]
    private void LoadSet(LibrarySet set)
    {
        ApplyOptions(set.Settings, set.Settings.Count);
        if (Workspace.Generate(set.Settings)) IsLibraryOpen = false;
    }

    [RelayCommand]
    private void LoadFolder()
    {
        if (Workspace.LoadFolder(LoadDirectory) is not { } set) return;
        ApplyOptions(set.Samples[0].Settings, set.Samples.Count);
        IsLibraryOpen = false;
    }

    [RelayCommand]
    private async Task ConfirmExport()
    {
        IsBusy = true;
        try
        {
            if (await Workspace.ExportAsync(new(ExportDestination))) IsExportOpen = false;
        }
        finally { IsBusy = false; }
    }
}
