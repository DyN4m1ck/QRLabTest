using QRLab.Core.Models.Images;

namespace QRLab.Core.Models.Mutations;

/// <summary>Кадр цепочки мутаций: применённые параметры и полученное изображение.</summary>
public sealed record MutationFrame(MutationOptions Options, RasterImage Image);
