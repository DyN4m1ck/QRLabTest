using QRLab.Core.Models.Samples;

namespace QRLab.Core.Models.Storage;

/// Сохраняемый/загружаемый набор: имя и все образцы с их состоянием.
public sealed record SampleSet(string Name, IReadOnlyList<SamplePreview> Samples);
