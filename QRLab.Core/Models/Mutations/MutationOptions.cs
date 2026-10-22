using System.Text.Json.Serialization;

namespace QRLab.Core.Models.Mutations;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$mutation")]
[JsonDerivedType(typeof(GaussianBlurOptions), "gaussian")]
[JsonDerivedType(typeof(InkMutationOptions), "ink")]
[JsonDerivedType(typeof(PerspectiveMutationOptions), "perspective")]
public abstract record MutationOptions
{
    [JsonIgnore] public abstract MutationKind Kind { get; }
    public abstract void Validate();
}