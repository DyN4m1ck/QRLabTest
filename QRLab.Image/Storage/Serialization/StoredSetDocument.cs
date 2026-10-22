namespace QRLab.Image.Storage.Serialization;

internal sealed record StoredSetDocument(int Version, StoredSample[] Samples);