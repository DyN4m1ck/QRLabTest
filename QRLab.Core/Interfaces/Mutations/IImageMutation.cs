using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Core.Interfaces.Mutations;

/// Одна конкретная мутация изображения (размытие, шум, перспектива).
/// Каждая реализация отвечает ровно за один вид искажения и объявляет его через <see cref="Kind"/>.
public interface IImageMutation
{
    /// Вид мутации — служит ключом, по которому процессор находит нужную реализацию.
    MutationKind Kind { get; }

    /// Применяет мутацию к изображению с указанными параметрами.
    /// Исходное изображение неизменяемо (<see cref="RasterImage"/>), поэтому возвращается
    /// либо новый экземпляр, либо тот же самый при «нулевой силе» эффекта.
    RasterImage Apply(RasterImage image, MutationOptions options);
}
