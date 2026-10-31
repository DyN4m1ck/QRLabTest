namespace QRLab.Core.Models.Mutations;

/// Параметры импульсного шума «соль и перец»: какой доля пикселей управлять
/// и какая часть из них станет белой («соль») вместо чёрного («перец»).
/// Seed задаёт детерминированность: те же параметры — тот же рисунок шума.
public sealed record SaltAndPepperOptions : MutationOptions
{
    /// Вид мутации — импульсный шум.
    public override MutationKind Kind => MutationKind.SaltAndPepper;
    /// Плотность шума: % пикселей, которые будут заменены, 0..100.
    public double DensityPercent { get; init; } = MutationDefaults.NoisePercent;
    /// Доля белых точек среди зашумлённых пикселей, %, 0..100.
    public double SaltPercent { get; init; } = MutationDefaults.SaltPercent;
    /// Зерно ГСЧ для воспроизводимости результата.
    public int Seed { get; init; } = MutationDefaults.Seed;

    /// Проверяет конечность и процентные диапазоны обоих параметров.
    public override void Validate()
    {
        if (!double.IsFinite(DensityPercent) || DensityPercent is < 0 or > 100
            || !double.IsFinite(SaltPercent) || SaltPercent is < 0 or > 100)
            throw new ArgumentException("Плотность шума и доля белых точек должны быть от 0 до 100%.");
    }
}
