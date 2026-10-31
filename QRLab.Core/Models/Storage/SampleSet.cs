using QRLab.Core.Models.Samples;

namespace QRLab.Core.Models.Storage;

/// <summary>Сохраняемый/загружаемый набор: имя и все образцы с их состоянием.</summary>
public sealed record SampleSet(string Name, IReadOnlyList<SamplePreview> Samples);
