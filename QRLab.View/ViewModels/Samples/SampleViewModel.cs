using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QRLab.Core.Models.Generation.Qrcode;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;
using QRLab.View.Presentation;
using ResultStatus = QRLab.Core.Models.Recognition.Status;

namespace QRLab.View.ViewModels;

public sealed partial class SampleViewModel : ObservableObject
{
    private readonly SamplePreview _source;
    public SamplePreview Preview => _source with
    {
        Frames = Operations.Select(operation => new MutationFrame(operation.Options!, operation.Image)).ToArray()
    };
    public SampleViewModel(SamplePreview preview)
    {
        _source = preview;
        OriginalFrame = new("00", null, preview.Image);
        foreach (var (frame, index) in preview.Frames.Select((frame, index) => (frame, index)))
            Operations.Add(new($"{index + 1:00}", frame.Options, frame.Image));
        Operations.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Image));
            Result = null;
        };
    }
    public string Name => _source.Name;
    public string Content => _source.Content;
    public string Dimensions => $"{OriginalImage.Width} × {OriginalImage.Height} px";
    public string Correction => _source.Settings is QrGenerationOptions qr ? GenerationChoices.Name(GenerationChoices.Corrections, qr.Correction) : "—";
    public string Format => GenerationChoices.CodeName(_source.Settings);
    public RasterImage OriginalImage => _source.Image;
    public RasterImage Image => Operations.LastOrDefault()?.Image ?? OriginalImage;
    public OperationViewModel OriginalFrame { get; }
    public ObservableCollection<OperationViewModel> Operations { get; } = [];
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Status), nameof(IsFailure), nameof(Time), nameof(Chain), nameof(Decoded))]
    private RecognitionPreview? _result;

    public string Status => Result?.Status switch
    {
        null => "Не обработан",
        ResultStatus.Success => "Считан",
        ResultStatus.Failed => "Не считан",
        ResultStatus.Timeout => "Таймаут",
        _ => throw new InvalidOperationException()
    };
    public bool IsFailure => Result is not { Status: ResultStatus.Success };
    public string Time => Result is null ? "—" : $"{Result.Milliseconds:N1} мс";
    public string Chain => Result is null ? "Ожидает распознавания" : string.Join(" → ", Operations.Select(o => o.Name));
    public string Decoded => Result is { Status: ResultStatus.Success } ? Content : "—";
}