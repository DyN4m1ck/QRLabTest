using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

public sealed record Ean13GenerationOptions : LinearBarcodeGenerationOptions
{
    public override BarcodeType BarcodeType => BarcodeType.Ean13;

    public override string PrepareContent(int number)
    {
        var content = base.PrepareContent(number);
        if (content.Length is not (12 or 13) || content.Any(c => c is < '0' or > '9'))
            throw new ArgumentException("EAN-13: введите 12 цифр без контрольной суммы или 13 цифр с верной контрольной суммой (0–9).");
        var sum = 0;
        for (var i = 0; i < 12; i++) sum += (content[i] - '0') * (i % 2 == 0 ? 1 : 3);
        var check = (char)('0' + (10 - sum % 10) % 10);
        if (content.Length == 13 && content[12] != check)
            throw new ArgumentException($"EAN-13: неверная контрольная сумма, ожидается последняя цифра {check}.");
        return content[..12] + check;
    }
}
