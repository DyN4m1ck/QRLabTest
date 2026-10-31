namespace QRLab.Core.Models.Mutations;

/// Способ интерполяции при повторной выборке (ресемплинге) изображения.
public enum ImageInterpolation { Bicubic, Bilinear, Nearest }

/// Параметры перспективного искажения: два угла поворота «плоскости» изображения
/// и расстояние виртуальной камеры (чем меньше — тем сильнее трапеция).
public sealed record PerspectiveMutationOptions : MutationOptions
{
    /// Вид мутации — перспектива.
    public override MutationKind Kind => MutationKind.Perspective;
    /// Угол поворота вокруг оси X, градусов (-60..60).
    public double AngleX { get; init; } = MutationDefaults.AngleX;
    /// Угол поворота вокруг оси Y, градусов (-60..60).
    public double AngleY { get; init; } = MutationDefaults.AngleY;
    /// Расстояние камеры (множитель фокусного), 1..10.
    public double Distance { get; init; } = MutationDefaults.Distance;
    /// Фильтр интерполяции при обратном проходе лучей.
    public ImageInterpolation Interpolation { get; init; } = ImageInterpolation.Bicubic;

    /// Проверяет конечность углов, их диапазон, расстояние и интерполяцию.
    public override void Validate()
    {
        if (!double.IsFinite(AngleX) || Math.Abs(AngleX) > MutationDefaults.MaximumAngle
            || !double.IsFinite(AngleY) || Math.Abs(AngleY) > MutationDefaults.MaximumAngle
            || !double.IsFinite(Distance) || Distance < MutationDefaults.MinimumDistance || Distance > MutationDefaults.MaximumDistance
            || !Enum.IsDefined(Interpolation))
            throw new ArgumentException("Недопустимые параметры перспективы.");
    }
}
