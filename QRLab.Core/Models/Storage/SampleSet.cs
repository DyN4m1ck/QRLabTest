using QRLab.Core.Models.Samples;

namespace QRLab.Core.Models.Storage;

public sealed record SampleSet(string Name, IReadOnlyList<SamplePreview> Samples);