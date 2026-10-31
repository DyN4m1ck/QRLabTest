using System.Text.Json;
using QRLab.Core.Interfaces.Storage;
using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.Core.Models.Samples;
using QRLab.Core.Models.Storage;
using QRLab.Image.Storage.Serialization;

namespace QRLab.Image.Storage;

// Хранилище наборов образцов на диске: JSON-манифест qrlab-set.json + PNG-файлы рядом.
// Принципы: (1) версионирование формата (CurrentVersion=3) — наборы другой версии отвергаются
// явно; (2) лимиты размеров против повреждённых файлов; (3) атомарность сохранения
// через staging-папку и Directory.Move; (4) при загрузке — полная повторная валидация данных.
public sealed class SampleSetStorage : ISampleSetStorage
{
    // Имя манифеста — контракт формата; Load ищет именно его в выбранной папке.
    public const string ManifestName = "qrlab-set.json";
    // Версия схемы сериализации. Меняется при любом несовместимом изменении Stored*-записей.
    private const int CurrentVersion = 3;
    // Потолок размера манифеста защищает от чтения гигантского/повреждённого JSON в память.
    private const int MaximumManifestBytes = 64 * 1024 * 1024;
    private const int MaximumImageBytes = 512 * 1024 * 1024;
    // Отступы делают файл читаемым для человека — набор можно отредактировать руками.
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // Сохранение всей цепочки: оригиналы и все кадры мутаций каждого образца.
    public SampleSet Save(IReadOnlyList<SamplePreview> samples, ExportOptions options)
    {
        // Количество сверяется с теми же границами Core, что и при генерации — единый источник правды.
        if (samples.Count is < GenerationLimits.MinimumCount or > GenerationLimits.MaximumCount)
            throw new ArgumentException("Недопустимое количество образцов.");
        if (string.IsNullOrWhiteSpace(options.Destination)) throw new ArgumentException("Укажите папку набора.");
        var directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.Destination));
        var parent = Path.GetDirectoryName(directory);
        if (parent is null) throw new ArgumentException("Выберите отдельную папку набора.");
        // Разрешаем писать только в новую или пустую папку: нельзя случайно перезаписать чужие файлы.
        if (Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any())
            throw new IOException("Папка не пуста. Укажите новую или пустую папку для сохранения набора.");
        Directory.CreateDirectory(parent);
        // Пишем во временную папку-«черновик» рядом с целевой; если что-то упадёт посередине,
        // конечная папка останется нетронутой (staging удаляется в finally).
        var staging = Path.Combine(parent, $".qrlab-{Guid.NewGuid():N}");
        Directory.CreateDirectory(staging);
        try
        {
            // Дедупликация по ссылке: несколько кадров с одинаковым объектом RasterImage
            // («нулевые» мутации возвращают исходный объект без изменений) пишутся на диск один раз.
            var images = new Dictionary<RasterImage, string>(ReferenceEqualityComparer.Instance);
            // Локальная функция записи PNG: проверили кэш -> записали -> запомнили имя файла.
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
                // Человекочитаемые имена: sample-0001.png, sample-0001-frame-0001.png —
                // порядок нумерации виден прямо в проводнике.
                var prefix = $"sample-{index + 1:0000}";
                var image = WriteImage(sample.Image, prefix + RasterImage.FileExtension);
                var frames = sample.Frames.Select((frame, frameIndex) => new StoredMutation(frame.Options,
                    WriteImage(frame.Image, $"{prefix}-frame-{frameIndex + 1:0000}{RasterImage.FileExtension}"))).ToArray();
                return new StoredSample(sample.Number, sample.Name, sample.Content, sample.Settings, image, frames);
            }).ToArray();
            // Манифест пишется последним: он ссылается на уже существующие файлы изображений.
            File.WriteAllText(Path.Combine(staging, ManifestName),
                JsonSerializer.Serialize(new StoredSetDocument(CurrentVersion, stored), JsonOptions));
            if (Directory.Exists(directory)) Directory.Delete(directory);
            // Атомарная публикация всего набора одним переименованием в пределах тома.
            Directory.Move(staging, directory);
        }
        finally
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true);
        }
        return new(Path.GetFileName(directory), samples);
    }

    // Загрузка: прочитать манифест -> проверить версию и лимиты -> подтянуть PNG ->
    // восстановить полиморфные опции -> повторно валидировать всё и собрать SampleSet.
    public SampleSet Load(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Укажите папку сохранённого набора.");
        directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        var manifest = Path.Combine(directory, ManifestName);
        if (!File.Exists(manifest)) throw new IOException($"В папке отсутствует {ManifestName}.");
        // Размер проверяем ДО ReadAllText — иначе можно получить OutOfMemory на битом файле.
        if (new FileInfo(manifest).Length > MaximumManifestBytes) throw new InvalidDataException("Описание набора слишком велико.");
        StoredSetDocument document;
        try
        {
            document = JsonSerializer.Deserialize<StoredSetDocument>(File.ReadAllText(manifest), JsonOptions)
                ?? throw new InvalidDataException("Пустое описание набора.");
        }
        // Любая ошибка парсинга приводится к единому типу InvalidDataException для UI-обработчика.
        catch (JsonException exception) { throw new InvalidDataException("Описание набора повреждено.", exception); }
        // Строгая политика совместимости вместо эвристик «попробуем прочитать как получится».
        if (document.Version != CurrentVersion) throw new InvalidDataException("Версия формата набора не поддерживается.");
        if (document.Samples is not { Length: >= GenerationLimits.MinimumCount and <= GenerationLimits.MaximumCount })
            throw new InvalidDataException("Недопустимые параметры набора.");

        // Кэш по имени файла: один PNG читается один раз даже на десятки ссылок из манифеста.
        var images = new Dictionary<string, RasterImage>(StringComparer.Ordinal);
        RasterImage ReadImage(string name)
        {
            // Защита от path traversal: имя обязано быть простым именем файла .png без подпапок —
            // вручную отредактированный манифест не должен дать прочитать произвольный файл диска.
            if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name)
                || name.Contains('\\') || !name.EndsWith(RasterImage.FileExtension, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Недопустимое имя изображения.");
            if (images.TryGetValue(name, out var cached)) return cached;
            var path = Path.Combine(directory, name);
            // Отдельный (больший) лимит на растровые данные.
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
                // Параметры генерации из файла проходят ту же валидацию, что и созданные в UI:
                // набор, загруженный с диска, не имеет права содержать невозможные значения.
                stored.Settings.Validate();
                var original = ReadImage(stored.Image);
                // Кросс-проверка целостности: реальный размер PNG обязан совпадать с заявленным.
                if (original.Width != stored.Settings.Width || original.Height != stored.Settings.Height)
                    throw new InvalidDataException("Размер изображения не соответствует параметрам образца.");
                var frames = stored.Frames.Select(frame =>
                {
                    if (frame?.Options is null) throw new InvalidDataException("Некорректная операция.");
                    // Опции мутаций тоже полиморфны и валидируются; битый кадр = ошибка всего набора.
                    frame.Options.Validate();
                    return new MutationFrame(frame.Options, ReadImage(frame.Image));
                }).ToArray();
                return new SamplePreview(stored.Number, stored.Name, stored.Content, original, frames, stored.Settings);
            }).ToArray();
            // Номера образцов уникальны — иначе ссылки в отчётах и UI стали бы неоднозначными.
            if (samples.Select(s => s.Number).Distinct().Count() != samples.Length)
                throw new InvalidDataException("В наборе повторяются номера образцов.");
            return new(Path.GetFileName(directory), samples);
        }
        // Ошибки валидации моделей конвертируем в тот же тип, что ожидает слой представления.
        catch (ArgumentException exception) { throw new InvalidDataException("Некорректные данные набора.", exception); }
    }
}