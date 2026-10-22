using QRLab.Core.Models.Generation;

namespace QRLab.Image.Storage.Serialization;

internal sealed record StoredSample(int Number, string Name, string Content, GenerationOptions Settings,
    string Image, StoredMutation[] Frames);