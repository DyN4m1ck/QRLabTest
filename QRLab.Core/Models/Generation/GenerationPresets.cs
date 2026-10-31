using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Встроенная «библиотека наборов» — готовые пресеты, которые пользователь
/// может применить одним кликом на экране генерации.
/// </summary>
public static class GenerationPresets
{
    /// <summary>Список доступных пресетов.</summary>
    public static IReadOnlyList<LibrarySet> Library { get; } =
        Array.AsReadOnly<LibrarySet>([
        new("Тестовый набор", new QrGenerationOptions { Count = 12, Template = "TEST-{num}" }),
    ]);
}
