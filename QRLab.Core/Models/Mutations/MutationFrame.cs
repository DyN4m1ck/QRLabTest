using QRLab.Core.Models.Images;

namespace QRLab.Core.Models.Mutations;

/// Кадр цепочки мутаций: применённые параметры и полученное изображение.
public sealed record MutationFrame(MutationOptions Options, RasterImage Image);
