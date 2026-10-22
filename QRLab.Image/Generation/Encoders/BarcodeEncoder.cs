using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Generation.Barcode;
using ZXing;

namespace QRLab.Image.Generation.Encoders;

public sealed class BarcodeEncoder : CodeEncoder
{
    private static readonly IReadOnlyDictionary<BarcodeType, BarcodeFormat> Formats = new Dictionary<BarcodeType, BarcodeFormat>
    {
        [BarcodeType.Ean13] = BarcodeFormat.EAN_13,
        [BarcodeType.Code128] = BarcodeFormat.CODE_128,
        [BarcodeType.Pdf417] = BarcodeFormat.PDF_417
    };

    public override bool Supports(GenerationOptions options) => options is BarcodeGenerationOptions barcode && Formats.ContainsKey(barcode.BarcodeType);
    protected override BarcodeFormat GetFormat(GenerationOptions options) => Formats[((BarcodeGenerationOptions)options).BarcodeType];

    protected override IDictionary<EncodeHintType, object> CreateHints(GenerationOptions options)
    {
        var hints = base.CreateHints(options);
        if (options is Pdf417GenerationOptions) hints[EncodeHintType.CHARACTER_SET] = "UTF-8";
        return hints;
    }
}
