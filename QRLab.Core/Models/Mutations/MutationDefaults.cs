namespace QRLab.Core.Models.Mutations;

/// Значения по умолчанию и границы параметров всех мутаций.
public static class MutationDefaults
{
    /// Sigma размытия по умолчанию, px.
    public const double Sigma = 1.2;
    /// Минимальная sigma (0 — эффект не применяется).
    public const double MinimumSigma = 0;
    /// Максимальная sigma.
    public const double MaximumSigma = 50;
    /// Угол наклона перспективы вокруг X по умолчанию, градусы.
    public const double AngleX = 8;
    /// Угол наклона перспективы вокруг Y по умолчанию, градусы.
    public const double AngleY = -4;
    /// Максимальный модуль угла перспективы.
    public const double MaximumAngle = 60;
    /// Относительное расстояние камеры по умолчанию.
    public const double Distance = 2;
    /// Минимальное расстояние камеры.
    public const double MinimumDistance = 1;
    /// Максимальное расстояние камеры.
    public const double MaximumDistance = 10;
    /// Плотность шума «соль и перец» по умолчанию, % пикселей.
    public const double NoisePercent = 1;
    /// Доля белых точек среди зашумлённых пикселей по умолчанию, %.
    public const double SaltPercent = 50;
    /// Зерно генератора случайных чисел по умолчанию (детерминизм).
    public const int Seed = 42;
}
