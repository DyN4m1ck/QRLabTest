using System.Text.Json;
using QRLab.Core.Interfaces.Storage;
using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.Core.Models.Samples;
using QRLab.Core.Models.Storage;
using QRLab.Image.Storage.Serialization;

namespace QRLab.Image.Storage;

public sealed class SampleSetStorage : ISampleSetStorage
{
    public const string ManifestName = "qrlab-set.json";
    private const int CurrentVersion = 3;
    private const int MaximumManifestBytes = 64 * 1024 * 1024;
    private const int MaximumImageBytes = 512 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SampleSet Save(IReadOnlyList<SamplePreview> samples, ExportOptions options)
    {
        if (samples.Count is < GenerationLimits.MinimumCount or > GenerationLimits.MaximumCount)
            throw new ArgumentException("Недопустимое количество образцов.");
        if (string.IsNullOrWhiteSpace(options.Destination)) throw new ArgumentException("Укажите папку набора.");
        var directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.Destination));
        var parent = Path.GetDirectoryName(directory);
        if (parent is null) throw new ArgumentException("Выберите отдельную папку набора.");
        if (Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any())
            throw new IOException("Папка не пуста. Укажите новую или пустую папку для сохранения набора.");
        Directory.CreateDirectory(parent);
        var staging = Path.Combine(parent, $".qrlab-{Guid.NewGuid():N}");
        Directory.CreateDirectory(staging);
        try
        {
            var images = new Dictionary<RasterImage, string>(ReferenceEqualityComparer.Instance);
            string WriteImage(RasterImage image, string name)
            {
                if (images.TryGetValue(image, out var saved)) return saved;
                using var stream = File.Create(Path.Combine(staging, name));
                image.WriteTo(stream);
                images.Add(image, name);
                return name;
            }
            var stored = samples.Select((sample, index) =>
            {
                var prefix = $"sample-{index + 1:0000}";
                var image = WriteImage(sample.Image, prefix + RasterImage.FileExtension);
                var frames = sample.Frames.Select((frame, frameIndex) => new StoredMutation(frame.Options,
                    WriteImage(frame.Image, $"{prefix}-frame-{frameIndex + 1:0000}{RasterImage.FileExtension}"))).ToArray();
                return new StoredSample(sample.Number, sample.Name, sample.Content, sample.Settings, image, frames);
            }).ToArray();
            File.WriteAllText(Path.Combine(staging, ManifestName),
                JsonSerializer.Serialize(new StoredSetDocument(CurrentVersion, stored), JsonOptions));
            if (Directory.Exists(directory)) Directory.Delete(directory);
            Directory.Move(staging, directory);
        }
        finally
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true);
        }
        return new(Path.GetFileName(directory), samples);
    }

    public SampleSet Load(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Укажите папку сохранённого набора.");
        directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        var manifest = Path.Combine(directory, ManifestName);
        if (!File.Exists(manifest)) throw new IOException($"В папке отсутствует {ManifestName}.");
        if (new FileInfo(manifest).Length > MaximumManifestBytes) throw new InvalidDataException("Описание набора слишком велико.");
        StoredSetDocument document;
        try
        {
            document = JsonSerializer.Deserialize<StoredSetDocument>(File.ReadAllText(manifest), JsonOptions)
                ?? throw new InvalidDataException("Пустое описание набора.");
        }
        catch (JsonException exception) { throw new InvalidDataException("Описание набора повреждено.", exception); }
        if (document.Version != CurrentVersion) throw new InvalidDataException("Версия формата набора не поддерживается.");
        if (document.Samples is not { Length: >= GenerationLimits.MinimumCount and <= GenerationLimits.MaximumCount })
            throw new InvalidDataException("Недопустимые параметры набора.");

        var images = new Dictionary<string, RasterImage>(StringComparer.Ordinal);
        RasterImage ReadImage(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name)
                || name.Contains('\\') || !name.EndsWith(RasterImage.FileExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Недопустимое имя изображения.");
            if (images.TryGetValue(name, out var cached)) return cached;
            var path = Path.Combine(directory, name);
            if (new FileInfo(path).Length > MaximumImageBytes) throw new InvalidDataException("Изображение слишком велико.");
            var image = new RasterImage(File.ReadAllBytes(path));
            images.Add(name, image);
            return image;
        }

        try
        {
            var samples = document.Samples.Select(stored =>
            {
                if (stored is null || stored.Number < GenerationDefaults.FirstNumber || string.IsNullOrWhiteSpace(stored.Name)
                    || string.IsNullOrEmpty(stored.Content) || stored.Settings is null || stored.Frames is null)
                    throw new InvalidDataException("Некорректные данные образца.");
                stored.Settings.Validate();
                var original = ReadImage(stored.Image);
                if (original.Width != stored.Settings.Width || original.Height != stored.Settings.Height)
                    throw new InvalidDataException("Размер изображения не соответствует параметрам образца.");
                var frames = stored.Frames.Select(frame =>
                {
                    if (frame?.Options is null) throw new InvalidDataException("Некорректная операция.");
                    frame.Options.Validate();
                    return new MutationFrame(frame.Options, ReadImage(frame.Image));
                }).ToArray();
                return new SamplePreview(stored.Number, stored.Name, stored.Content, original, frames, stored.Settings);
            }).ToArray();
            if (samples.Select(s => s.Number).Distinct().Count() != samples.Length)
                throw new InvalidDataException("В наборе повторяются номера образцов.");
            return new(Path.GetFileName(directory), samples);
        }
        catch (ArgumentException exception) { throw new InvalidDataException("Некорректные данные набора.", exception); }
    }
}