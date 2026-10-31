namespace QRLab.Core.Models.Mutations;

/// <summary>Способ интерполяции при перспективной пересэмплинг картинки.</summary>
public enum ImageInterpolation { Bicubic, Bilinear, Nearest }

/// <summary>
/// Параметры перспективного искажения: два угла поворота «плоскости» изображения
/// и расстояние виртуальной камеры (чем меньше — тем сильнее трапеция).
/// </summary>
public sealed record PerspectiveMutationOptions : MutationOptions
{
    /// <summary>Вид мутации — перспектива.</summary>
    public override MutationKind Kind => MutationKind.Perspective;
    /// <summary>Угол поворота вокруг оси X, градусы (-60..60).</summary>
    public double AngleX { get; init; } = MutationDefaults.AngleX;
    /// <summary>Угол поворота вокруг оси Y, градусы (-60..60).</summary>
    public double AngleY { get; init; } = MutationDefaults.AngleY;
    /// <summary>Расстояние камеры (множитель фокусного), 1..10.</summary>
    public double Distance { get; init; } = MutationDefaults.Distance;
    /// <summary>Фильтр интерполяции при обратном проходе лучей.</summary>
    public ImageInterpolation Interpolation { get; init; } = ImageInterpolation.Bicubic;

    /// <summary>Проверяет конечность углей, их диапазон, расстояние и интерполяцию.</summary>
    public override void Validate()
    {
        if (!double.IsFinite(AngleX) || Math.Abs(AngleX) > MutationDefaults.MaximumAngle
            || !double.IsFinite(AngleY) || Math.Abs(AngleY) > MutationDefaults.MaximumAngle
            || !double.IsFinite(Distance) || Distance < MutationDefaults.MinimumDistance || Distance > MutationDefaults.MaximumDistance
            || !Enum.IsDefined(Interpolation))
            throw new ArgumentException("Недопустимые параметры перспективы.");
    }
}
