namespace QRLab.Core.Models.Generation;

/// Готовый пресет из «библиотеки наборов»: отображаемое имя и параметры генерации.
public sealed record LibrarySet(string Name, GenerationOptions Settings);
