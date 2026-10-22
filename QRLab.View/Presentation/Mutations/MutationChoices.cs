using QRLab.Core.Models.Mutations;

namespace QRLab.View.Presentation.Mutations;

public static class MutationChoices
{
    public static IReadOnlyList<EnumOption<MutationMode>> Modes { get; } = Array.AsReadOnly<EnumOption<MutationMode>>(
        [new(MutationMode.Manual, "Вручную"), new(MutationMode.Algorithm, "Алгоритм")]);
    public static IReadOnlyList<EnumOption<MutationTool>> Tools { get; } = Array.AsReadOnly<EnumOption<MutationTool>>(
        [new(MutationTool.Brush, "Кисть", "brush"), new(MutationTool.Eraser, "Ластик", "erase"), new(MutationTool.Hand, "Рука", "hand")]);
    public static IReadOnlyList<EnumOption<ImageInterpolation>> Interpolations { get; } = Array.AsReadOnly<EnumOption<ImageInterpolation>>(
        [new(ImageInterpolation.Bicubic, "Бикубическая"), new(ImageInterpolation.Bilinear, "Билинейная"), new(ImageInterpolation.Nearest, "Ближайший сосед")]);
    public static IReadOnlyList<EnumOption<MutationKind>> Operations { get; } = Array.AsReadOnly<EnumOption<MutationKind>>(
        [new(MutationKind.GaussianBlur, "Размытие Гаусса", "drop"), new(MutationKind.Ink, "Кляксы", "droplets"), new(MutationKind.Perspective, "Перспектива", "selection")]);
    public static FrameLabel Original { get; } = new("Исходник", "Защищён");
    public static FrameLabel Placeholder { get; } = new("Без изменений", "Операция пока не изменяет изображение.");
    public static string Describe(MutationOptions options) => options is GaussianBlurOptions blur
        ? $"σ = {blur.Sigma:N1} px" : Placeholder.Description;
}