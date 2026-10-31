namespace QRLab.Image.Storage.Serialization;

// Корень JSON-манифеста: версия формата + все образцы набора.
internal sealed record StoredSetDocument(int Version, StoredSample[] Samples);