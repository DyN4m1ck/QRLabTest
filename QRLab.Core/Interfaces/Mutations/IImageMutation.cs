using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Core.Interfaces.Mutations;

/// <summary>
/// Одна конкретная мутация изображения (размытие, шум, перспектива).
/// Каждая реализация отвечает ровно за один вид искажения и объявляет его через <see cref="Kind"/>.
/// </summary>
public interface IImageMutation
{
    /// <summary>Вид мутации — служит ключом, по которому процессор находит нужную реализацию.</summary>
    MutationKind Kind { get; }

    /// <summary>
    /// Применяет мутацию к изображению с указанными параметрами.
    /// Исходное изображение неизменяемо (<see cref="RasterImage"/>), поэтому возвращается
    /// либо новый экземпляр, либо тот же самый при «нулевой силе» эффекта.
    /// </summary>
    RasterImage Apply(RasterImage image, MutationOptions options);
}
