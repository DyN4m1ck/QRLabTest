namespace QRLab.Core.Models.Mutations;

/// <summary>
/// Параметры импульсного шума «соль и перец»: какой доля пикселей управлять
/// и какая часть из них станет белой («соль») вместо чёрного («перец»).
/// Seed задаёт детерминированность: те же параметры — тот же рисунок шума.
/// </summary>
public sealed record SaltAndPepperOptions : MutationOptions
{
    /// <summary>Вид мутации — импульсный шум.</summary>
    public override MutationKind Kind => MutationKind.SaltAndPepper;
    /// <summary>Плотность шума: % пикселей, которые будут заменены, 0..100.</summary>
    public double DensityPercent { get; init; } = MutationDefaults.NoisePercent;
    /// <summary>Доля белых точек среди зашумлённых пикселей, %, 0..100.</summary>
    public double SaltPercent { get; init; } = MutationDefaults.SaltPercent;
    /// <summary>Зерно ГСЧ для воспроизводимости результата.</summary>
    public int Seed { get; init; } = MutationDefaults.Seed;

    /// <summary>Проверяет конечность и процентные диапазоны обоих параметров.</summary>
    public override void Validate()
    {
        if (!double.IsFinite(DensityPercent) || DensityPercent is < 0 or > 100
            || !double.IsFinite(SaltPercent) || SaltPercent is < 0 or > 100)
            throw new ArgumentException("Плотность шума и доля белых точек должны быть от 0 до 100%.");
    }
}
