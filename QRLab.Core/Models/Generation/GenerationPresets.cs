using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

public static class GenerationPresets
{
    public static IReadOnlyList<LibrarySet> Library { get; } = Array.AsReadOnly<LibrarySet>(
    [
        new("Тестовый набор", new QrGenerationOptions { Count = 12, Template = "TEST-{num}" }),
    ]);
}
