namespace QRLab.Core.Models.Mutations;

/// <summary>Значения по умолчанию и границы параметров всех мутаций.</summary>
public static class MutationDefaults
{
    /// <summary>Sigma размытия по умолчанию, px.</summary>
    public const double Sigma = 1.2;
    /// <summary>Минимальная sigma (0 — эффект не применяется).</summary>
    public const double MinimumSigma = 0;
    /// <summary>Максимальная sigma.</summary>
    public const double MaximumSigma = 50;
    /// <summary>Угол наклона перспективы вокруг X по умолчанию, градусы.</summary>
    public const double AngleX = 8;
    /// <summary>Угол наклона перспективы вокруг Y по умолчанию, градусы.</summary>
    public const double AngleY = -4;
    /// <summary>Максимальный модуль угла перспективы.</summary>
    public const double MaximumAngle = 60;
    /// <summary>Относительное расстояние камеры по умолчанию.</summary>
    public const double Distance = 2;
    /// <summary>Минимальное расстояние камеры.</summary>
    public const double MinimumDistance = 1;
    /// <summary>Максимальное расстояние камеры.</summary>
    public const double MaximumDistance = 10;
    /// <summary>Плотность шума «соль и перец» по умолчанию, % пикселей.</summary>
    public const double NoisePercent = 1;
    /// <summary>Доля белых точек среди зашумлённых пикселей по умолчанию, %.</summary>
    public const double SaltPercent = 50;
    /// <summary>Зерно генератора случайных чисел по умолчанию (детерминизм).</summary>
    public const int Seed = 42;
}
