using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;

namespace QRLab.Core.Interfaces.Recognition;

public interface ICodeRecognizer
{
    IReadOnlyList<RecognitionPreview> Recognize(IReadOnlyList<SamplePreview> samples, RecognitionOptions options);
}