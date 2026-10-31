using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// Встроенная «библиотека наборов» — готовые пресеты, которые пользователь
/// может применить одним кликом на экране генерации.
public static class GenerationPresets
{
    /// Список доступных пресетов.
    public static IReadOnlyList<LibrarySet> Library { get; } =
        Array.AsReadOnly<LibrarySet>([
        new("Тестовый набор", new QrGenerationOptions { Count = 12, Template = "TEST-{num}" }),
    ]);
}
