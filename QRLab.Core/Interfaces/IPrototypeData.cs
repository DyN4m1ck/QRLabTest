namespace QRLab.Core.Services;

public sealed record SamplePreview(
    int Number, 
    string Name, 
    string Content, 
    string Format, 
    bool[,] Modules,
    IReadOnlyList<MutationPreview> Operations,
    GenerationOptions Settings
    );
public sealed record MutationPreview(
    string Number,
    string Name,
    string Details,
    int Stage
    );
public sealed record PreviewPoint(double X, double Y);
public sealed record FrameOptions(
    int Stage,
    double AngleX = 8,
    double AngleY = -4,
    double Blur = 1.2,
    double BrushSize = 48,
    double Opacity = 100,
    double CenterX = .62,
    double CenterY = .48,
    bool Erase = false,
    int Seed = 42,
    string Interpolation = "Бикубическая"
    );

public enum Status { Success, Failed, Timeout };
public sealed record RecognitionPreview(int Number, Status Status, double Milliseconds, string Chain);
public sealed record GenerationOptions(string Template, int FirstNumber, int Count, string Format, int Width = 1920, int Height = 1080, string Correction = "H • 30%", int QuietZone = 4, bool UseTemplate = true);
/// <summary>Data boundary for the prototype. The real engine can replace its provider.</summary>
public sealed record LibrarySet(string Name, int Count, string Template);
public sealed record ExportOptions(string Destination, string Format, bool IncludeMetadata);
public sealed record RecognitionOptions(string Decoder, int Timeout, bool Enhanced);
public interface IPrototypeData
{
    IReadOnlyList<LibrarySet> LibrarySets { get; }
    string Export(IReadOnlyList<SamplePreview> samples, ExportOptions options);
    FrameOptions CreateVariant(int seed, FrameOptions current);
    bool[,] CreateFrame(bool[,] original, FrameOptions options, bool difference = false);
    IReadOnlyList<SamplePreview> CreateSamples(GenerationOptions options);
    IReadOnlyList<RecognitionPreview> Recognize(IReadOnlyList<SamplePreview> samples, RecognitionOptions options);
}
