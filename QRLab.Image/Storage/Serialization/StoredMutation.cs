using QRLab.Core.Models.Mutations;

namespace QRLab.Image.Storage.Serialization;

internal sealed record StoredMutation(MutationOptions Options, string Image);