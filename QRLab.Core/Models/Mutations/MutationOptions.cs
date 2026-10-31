using System.Text.Json.Serialization;

namespace QRLab.Core.Models.Mutations;

/// Базовая запись параметров одной мутации. Как и <see cref="Generation.GenerationOptions"/>,
/// сериализуется полиморфно: в JSON пишется дискриминатор "$mutation"
/// ("gaussian", "salt-and-pepper", "perspective") для безопасного восстановления подтипа.
/// Конкретные наборы параметров живут в наследниках (GaussianBlurOptions и др.).
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$mutation")]
[JsonDerivedType(typeof(GaussianBlurOptions), "gaussian")]
[JsonDerivedType(typeof(SaltAndPepperOptions), "salt-and-pepper")]
[JsonDerivedType(typeof(PerspectiveMutationOptions), "perspective")]
public abstract record MutationOptions
{
    /// Вид мутации — определяет, какой IImageMutation её обработает.
    [JsonIgnore] public abstract MutationKind Kind { get; }

    /// Проверяет допустимость значений; вызывается перед применением эффекта.
    public abstract void Validate();
}
