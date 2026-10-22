using QRLab.Core.Models.Generation;
using ZXing;
using ZXing.Common;

namespace QRLab.Image.Generation.Encoders;

public abstract class CodeEncoder
{
    protected const int LinearMatrixHeight = 64;
    public abstract bool Supports(GenerationOptions options);
    protected abstract BarcodeFormat GetFormat(GenerationOptions options);

    protected virtual IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options) =>
        new Dictionary<EncodeHintType, object>
        {
            [EncodeHintType.MARGIN] = options.IsLinear ? options.QuietZone * 2 : options.QuietZone
        };

    public BitMatrix Encode(string content, GenerationOptions options) => new MultiFormatWriter().encode(
        content, GetFormat(options), 0, options.IsLinear ? LinearMatrixHeight : 0, CreateHints(options));
}
