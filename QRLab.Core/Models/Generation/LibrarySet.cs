namespace QRLab.Core.Models.Generation;

/// <summary>Готовый пресет из «библиотеки наборов»: отображаемое имя и параметры генерации.</summary>
public sealed record LibrarySet(string Name, GenerationOptions Settings);
