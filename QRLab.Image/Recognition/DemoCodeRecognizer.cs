using QRLab.Core.Interfaces.Recognition;
using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;

namespace QRLab.Image.Recognition;

/// <summary>Recognition remains a demo until the decoding stage is implemented.</summary>
public sealed class DemoCodeRecognizer : ICodeRecognizer
{
    public IReadOnlyList<RecognitionPreview> Recognize(IReadOnlyList<SamplePreview> samples, RecognitionOptions options) =>
        samples.Select(s => new RecognitionPreview(s.Number,
            s.Number % 5 != 2 ? Status.Success : s.Number % 3 != 0 ? Status.Failed : Status.Timeout,
            Math.Round(18 + s.Number * 7.13 % 33, 1),
            string.Join(" → ", s.Frames.Select(f => f.Options.Kind)))).ToArray();
}