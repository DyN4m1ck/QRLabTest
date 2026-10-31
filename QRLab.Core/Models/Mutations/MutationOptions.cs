using System.Text.Json.Serialization;

namespace QRLab.Core.Models.Mutations;

/// <summary>
/// Базовая запись параметров одной мутации. Как и <see cref="Generation.GenerationOptions"/>,
/// сериализуется полиморфно: в JSON пишется дискриминатор "$mutation"
/// ("gaussian", "salt-and-pepper", "perspective") для безопасного восстановления подтипа.
/// Конкретные наборы параметров живут в наследниках (GaussianBlurOptions и др.).
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$mutation")]
[JsonDerivedType(typeof(GaussianBlurOptions), "gaussian")]
[JsonDerivedType(typeof(SaltAndPepperOptions), "salt-and-pepper")]
[JsonDerivedType(typeof(PerspectiveMutationOptions), "perspective")]
public abstract record MutationOptions
{
    /// <summary>Вид мутации — определяет, какой IImageMutation её обработает.</summary>
    [JsonIgnore] public abstract MutationKind Kind { get; }

    /// <summary>Проверяет допустимость значений; вызывается перед применением эффекта.</summary>
    public abstract void Validate();
}
