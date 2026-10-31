#!/usr/bin/env python3
"""Вспомогательный скрипт: расставляет русские XML-комментарии по исходникам QRLab.
Запускать из корня репозитория. После применения скрипт можно удалить."""
import os, sys

ROOT = "/workspace"

def write(relpath, text):
    path = os.path.join(ROOT, relpath)
    assert os.path.isfile(path), f"NO SUCH FILE: {relpath}"
    with open(path, "w", encoding="utf-8") as f:
        f.write(text.lstrip("\n"))
    print("written:", relpath)

def run(parts):
    """parts — список: строка = путь к файлу (начало), либо текст-вставка."""
    current = None
    buf = []
    for item in parts:
        if isinstance(item, tuple) and item[0] == "file":
            if current is not None:
                write(current, "".join(buf))
            current, buf = item[1], []
        else:
            buf.append(item)
    if current is not None:
        write(current, "".join(buf))

# ============================ QRLab.Core: Interfaces ============================
run([
("file", "QRLab.Core/Interfaces/Generation/ICodeGenerator.cs"),
"""using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Samples;

namespace QRLab.Core.Interfaces.Generation;

/// <summary>
/// Абстракция генератора штрих- и QR-кодов.
/// Остальные слои зависят только от этого интерфейса и не знают о конкретной
/// библиотеке кодирования (ZXing), что позволяет подменять реализацию в тестах.
/// </summary>
public interface ICodeGenerator
{
    /// <summary>
    /// Создаёт набор образцов по параметрам генерации.
    /// Сначала проверяются параметры (<see cref="GenerationOptions.Validate"/>), затем
    /// генерируется ровно <see cref="GenerationOptions.Count"/> изображений.
    /// При некорректных параметрах выбрасывается <see cref="ArgumentException"/>.
    /// </summary>
    IReadOnlyList<SamplePreview> CreateSamples(GenerationOptions options);
}
""",
("file", "QRLab.Core/Interfaces/Mutations/IImageMutation.cs"),
"""using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Core.Interfaces.Mutations;

/// <summary>
/// Одна конкретная мутация изображения (размытие, шум, перспектива).
/// Каждая реализация отвечает ровно за один вид искажения и объявляет его через <see cref="Kind"/>.
/// </summary>
public interface IImageMutation
{
    /// <summary>Вид мутации — служит ключом, по которому процессор находит нужную реализацию.</summary>
    MutationKind Kind { get; }

    /// <summary>
    /// Применяет мутацию к изображению с указанными параметрами.
    /// Исходное изображение неизменяемо (<see cref="RasterImage"/>), поэтому возвращается
    /// либо новый экземпляр, либо тот же самый при «нулевой силе» эффекта.
    /// </summary>
    RasterImage Apply(RasterImage image, MutationOptions options);
}
""",
("file", "QRLab.Core/Interfaces/Mutations/IMutationProcessor.cs"),
"""using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Core.Interfaces.Mutations;

/// <summary>
/// Последовательный конвейер мутаций: применяет список операций одну за другой.
/// Результат каждой операции сохраняется отдельным кадром, чтобы интерфейс мог
/// показывать весь ход искажений, а не только финальную картинку.
/// </summary>
public interface IMutationProcessor
{
    /// <summary>
    /// Прогоняет изображение через цепочку операций и возвращает кадры
    /// (параметры + полученное после них изображение) в порядке применения.
    /// </summary>
    IReadOnlyList<MutationFrame> Apply(RasterImage original, IReadOnlyList<MutationOptions> operations);
}
""",
("file", "QRLab.Core/Interfaces/Recognition/ICodeRecognizer.cs"),
"""using QRLab.Core.Models.Recognition;
using QRLab.Core.Models.Samples;

namespace QRLab.Core.Interfaces.Recognition;

/// <summary>
/// Абстракция распознавателя кодов: декодирует содержимое с готовых изображений.
/// Реализация в QRLab.Image построена поверх ZXing; интерфейс позволяет заменить её
/// другой библиотекой или заглушкой без изменения ViewModel.
/// </summary>
public interface ICodeRecognizer
{
    /// <summary>
    /// Асинхронно распознаёт список образцов.
    /// Для каждого образца возвращается <see cref="RecognitionPreview"/> со статусом
    /// (успех, ошибка, тайм-аут, несовпадение), временем декодирования и считанным текстом.
    /// Поддерживается отмена через <paramref name="cancellationToken"/>.
    /// </summary>
    Task<IReadOnlyList<RecognitionPreview>> RecognizeAsync(IReadOnlyList<SamplePreview> samples, RecognitionOptions options, CancellationToken cancellationToken = default);
}
""",
("file", "QRLab.Core/Interfaces/Recognition/IRecognitionReportExporter.cs"),
"""using QRLab.Core.Models.Recognition;

namespace QRLab.Core.Interfaces.Recognition;

/// <summary>
/// Экспортёр отчёта о распознавании в файл (в текущей реализации — CSV).
/// Вынесен в интерфейс, чтобы формат отчёта можно было менять, не трогая ViewModel.
/// </summary>
public interface IRecognitionReportExporter
{
    /// <summary>
    /// Сохраняет строки отчёта по указанному пути. Операция асинхронная и отменяемая:
    /// запись выполняется во временный файл с последующей атомарной заменой,
    /// поэтому отмена не оставляет «полусозданный» отчёт.
    /// </summary>
    Task ExportAsync(string path, IReadOnlyList<RecognitionReportRow> rows, CancellationToken cancellationToken = default);
}
""",
("file", "QRLab.Core/Interfaces/Storage/ISampleSetStorage.cs"),
"""using QRLab.Core.Models.Samples;
using QRLab.Core.Models.Storage;

namespace QRLab.Core.Interfaces.Storage;

/// <summary>
/// Хранилище наборов образцов: сериализует их на диск и читает обратно.
/// Формат хранения — папка с PNG-изображениями и JSON-манифестом (qrlab-set.json).
/// </summary>
public interface ISampleSetStorage
{
    /// <summary>
    /// Сохраняет набор образцов в папку, указанную в <paramref name="options"/>.
    /// Реализация обязана писать данные атомарно (через staging-каталог) и никогда
    /// не перезаписывать существующие наборы. Возвращает сохранённый <see cref="SampleSet"/>.
    /// </summary>
    SampleSet Save(IReadOnlyList<SamplePreview> samples, ExportOptions options);

    /// <summary>
    /// Загружает ранее сохранённый набор из папки, полностью проверяя манифест
    /// и изображения. При повреждённых данных выбрасывается <see cref="InvalidDataException"/>.
    /// </summary>
    SampleSet Load(string directory);
}
""",
])

# ==================== QRLab.Core: Models (Generation / Mutations) ====================
run([
("file", "QRLab.Core/Models/Generation/GenerationOptions.cs"),
"""using System.Globalization;
using System.Text.Json.Serialization;
using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Базовая запись всех параметров генерации кода.
/// Помечена как полиморфная для System.Text.Json: при сохранении набора в манифест
/// пишется дискриминатор "$code" ("qr", "ean13", "code128", "pdf417"), что позволяет
/// восстановить конкретный подтип без反射 и безопасно при trimming.
/// Наследники добавляют специфичные для формата настройки и переопределяют
/// <see cref="PrepareContent"/> и <see cref="Validate"/>.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$code")]
[JsonDerivedType(typeof(QrGenerationOptions), "qr")]
[JsonDerivedType(typeof(Ean13GenerationOptions), "ean13")]
[JsonDerivedType(typeof(Code128GenerationOptions), "code128")]
[JsonDerivedType(typeof(Pdf417GenerationOptions), "pdf417")]
public abstract record GenerationOptions
{
    /// <summary>Род вида кода (QR или штрихкод); используется для выбора кодировщика.</summary>
    [JsonIgnore] public abstract CodeKind Kind { get; }
    /// <summary>Линейный ли код; влияет на масштабирование при рендере и отступы.</summary>
    [JsonIgnore] public virtual bool IsLinear => false;
    /// <summary>Префикс имени образца ("QR", "BAR" и т.п.).</summary>
    [JsonIgnore] public abstract string SamplePrefix { get; }

    /// <summary>Шаблон содержимого (например "LAB-2026-{num}") либо готовый текст.</summary>
    public string Template { get; init; } = GenerationDefaults.Template;
    /// <summary>Номер первого образца в пакете.</summary>
    public int FirstNumber { get; init; } = GenerationDefaults.FirstNumber;
    /// <summary>Сколько образцов сгенерировать.</summary>
    public int Count { get; init; } = GenerationDefaults.Count;
    /// <summary>Ширина итогового PNG в пикселях.</summary>
    public int Width { get; init; } = GenerationDefaults.Width;
    /// <summary>Высота итогового PNG в пикселях.</summary>
    public int Height { get; init; } = GenerationDefaults.Height;
    /// <summary>Тихая зона (отступ) вокруг кода в модулях.</summary>
    public int QuietZone { get; init; } = GenerationDefaults.QuietZone;
    /// <summary>Источник содержимого: шаблон с номером или произвольный текст.</summary>
    public ContentSource Source { get; init; } = GenerationDefaults.Source;

    /// <summary>
    /// Проверка корректности параметров перед генерацией. Бросает
    /// <see cref="ArgumentException"/> с русским сообщением, понятным пользователю UI.
    /// </summary>
    public virtual void Validate()
    {
        if (string.IsNullOrWhiteSpace(Template)) throw new ArgumentException("Введите содержимое или шаблон кода.");

        if (Count < GenerationLimits.MinimumCount || Count > GenerationLimits.MaximumCount
            || FirstNumber < GenerationDefaults.FirstNumber || (long)FirstNumber + Count - 1 > int.MaxValue)
            throw new ArgumentException($"Количество должно быть от {GenerationLimits.MinimumCount} до {GenerationLimits.MaximumCount}, первый номер — положительным.");

        if (Width < GenerationLimits.MinimumImageSize || Width > GenerationLimits.MaximumImageSize
            || Height < GenerationLimits.MinimumImageSize || Height > GenerationLimits.MaximumImageSize)
            throw new ArgumentException($"Размер изображения должен быть от {GenerationLimits.MinimumImageSize} до {GenerationLimits.MaximumImageSize} пикселей.");

        if (QuietZone is < GenerationLimits.MinimumQuietZone or > GenerationLimits.MaximumQuietZone)
            throw new ArgumentException($"Тихая зона должна быть от {GenerationLimits.MinimumQuietZone} до {GenerationLimits.MaximumQuietZone} модулей.");

        if (!Enum.IsDefined(Source)) throw new ArgumentException("Неизвестный источник содержимого.");
    }

    /// <summary>
    /// Готовит содержимое i-го образца: в режиме шаблона подставляет номер
    /// с фиксированной шириной ("0000"), в режиме текста возвращает шаблон как есть.
    /// </summary>
    public virtual string PrepareContent(int number) => Source == ContentSource.NumberedTemplate
        ? Template.Replace(GenerationDefaults.NumberPlaceholder, number.ToString(GenerationDefaults.NumberFormat, CultureInfo.InvariantCulture))
        : Template;

    /// <summary>Формирует имя файла/образца по его номеру, например "QR-0012".</summary>
    public string GetSampleName(int number) => $"{SamplePrefix}-{number.ToString(GenerationDefaults.NumberFormat, CultureInfo.InvariantCulture)}";
}
""",
("file", "QRLab.Core/Models/Mutations/MutationOptions.cs"),
"""using System.Text.Json.Serialization;

namespace QRLab.Core.Models.Mutations;

/// <summary>
/// Базовая запись параметров одной мутации. Как и <see cref="Generation.GenerationOptions"/>,
/// сериализуется полиморфно: в JSON пишется дискриминатор "$mutation"
/// ("gaussian", "salt-and-pepper", "perspective") для безопасного восстановления подтипа.
/// Конкретные наборы параметров живут в наследниках (GaussianBlurOptions и др.).
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$mutation")]
[JsonDerivedType(typeof(GaussianBlurOptions), "gaussian")]
[JsonDerivedType(typeof(SaltAndPepperOptions), "salt-and-pepper")]
[JsonDerivedType(typeof(PerspectiveMutationOptions), "perspective")]
public abstract record MutationOptions
{
    /// <summary>Вид мутации — определяет, какой IImageMutation её обработает.</summary>
    [JsonIgnore] public abstract MutationKind Kind { get; }

    /// <summary>Проверяет допустимость значений; вызывается перед применением эффекта.</summary>
    public abstract void Validate();
}
""",
])

# ==================== QRLab.Core: остальные модели (полные файлы) ====================
run([
("file", "QRLab.Core/Models/Format.cs"),
"""namespace QRLab.Core.Models;

/// <summary>
/// Устаревшая модель формата кода. Используется только legacy-классами
/// CodeSample/MutationStep и тестом ModelTests; рабочий код приложения
/// опирается на CodeKind и BarcodeType из пространства имён Generation.
/// </summary>
public enum Format { QrCode, Barcode }
""",
("file", "QRLab.Core/Models/CodeSample.cs"),
"""namespace QRLab.Core.Models;

/// <summary>
/// Устаревшая (legacy) модель образца кода из ранних версий проекта.
/// Сейчас полноценные образцы описываются записью SamplePreview;
/// этот класс оставлен и покрывается только тестом ModelTests.
/// </summary>
public sealed class CodeSample
{
    /// <summary>Уникальный идентификатор образца.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>Отображаемое имя.</summary>
    public string Name { get; set; } = "Новый образец";
    /// <summary>Кодируемое содержимое.</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Формат кода (QR или штрихкод).</summary>
    public Format Format { get; set; } = Format.QrCode;
    /// <summary>Цепочка описаний мутаций (без самих изображений).</summary>
    public List<MutationStep> Mutations { get; } = [];
}
""",
("file", "QRLab.Core/Models/MutationStep.cs"),
"""namespace QRLab.Core.Models;

/// <summary>
/// Устаревшее описание шага мутации: только имя и «сила», без параметров
/// и изображений. Реальные цепочки искажений хранятся как MutationFrame.
/// </summary>
public sealed class MutationStep
{
    /// <summary>Уникальный идентификатор шага.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>Название операции для отображения.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Интенсивность эффекта в произвольных единицах (0..1 у UI-слайдеров).</summary>
    public double Intensity { get; set; }
}
""",
("file", "QRLab.Core/Models/DecodeResult.cs"),
"""namespace QRLab.Core.Models;

/// <summary>
/// Устаревший результат декодирования одного изображения. Современный аналог —
/// RecognitionPreview; класс используется лишь тестом ModelTests.
/// </summary>
public sealed class DecodeResult
{
    /// <summary>Идентификатор результата.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    /// <summary>Идентификатор образца, к которому относится результат.</summary>
    public Guid SampleId { get; init; }
    /// <summary>Удалось ли считать код.</summary>
    public bool IsSuccess { get; init; }
    /// <summary>Считанный текст (если успех).</summary>
    public string? DecodedText { get; init; }
    /// <summary>Сообщение об ошибке (если провал).</summary>
    public string? ErrorMessage { get; init; }
    /// <summary>Длительность декодирования.</summary>
    public TimeSpan Duration { get; init; }
}
""",
])


# ==================== QRLab.Core: Generation (enum/records) ====================
run([
("file", "QRLab.Core/Models/Generation/CodeKind.cs"),
"""namespace QRLab.Core.Models.Generation;

/// <summary>Родовой вид кода: квадратный QR-код или любой штрихкод.</summary>
public enum CodeKind { QrCode, Barcode }
""",
("file", "QRLab.Core/Models/Generation/GenerationMode.cs"),
"""namespace QRLab.Core.Models.Generation;

/// <summary>Режим генерации: один код или пакет нумерованных образцов.</summary>
public enum GenerationMode { Single, Batch }
""",
("file", "QRLab.Core/Models/Generation/LibrarySet.cs"),
"""namespace QRLab.Core.Models.Generation;

/// <summary>Готовый пресет из «библиотеки наборов»: отображаемое имя и параметры генерации.</summary>
public sealed record LibrarySet(string Name, GenerationOptions Settings);
""",
("file", "QRLab.Core/Models/Generation/ContentSource.cs"),
"""namespace QRLab.Core.Models.Generation;

/// <summary>Источник содержимого кодов.</summary>
public enum ContentSource
{
    /// <summary>Шаблон строки с плейсхолдером {num}, который заменяется номером образца.</summary>
    NumberedTemplate,
    /// <summary>Произвольный текст без подстановки номера.</summary>
    Text
}
""",
("file", "QRLab.Core/Models/Generation/Qrcode/QrCorrectionLevel.cs"),
"""namespace QRLab.Core.Models.Generation.Qrcode;

/// <summary>Уровень коррекции ошибок QR-кода (по стандарту ISO/IEC 18004).</summary>
public enum QrCorrectionLevel
{
    /// <summary>L — восстанавливается ~7% кодовых слов.</summary>
    Low,
    /// <summary>M — восстанавливается ~15% кодовых слов.</summary>
    Medium,
    /// <summary>Q — восстанавливается ~25% кодовых слов.</summary>
    Quartile,
    /// <summary>H — восстанавливается ~30% кодовых слов (лучше всего для мутаций).</summary>
    High
}
""",
("file", "QRLab.Core/Models/Generation/Barcode/BarcodeType.cs"),
"""namespace QRLab.Core.Models.Generation.Barcode;

/// <summary>Поддерживаемые виды штрихкодов.</summary>
public enum BarcodeType
{
    /// <summary>Линейный европейский 13-значный код с контрольной суммой.</summary>
    Ean13,
    /// <summary>Линейный алфавитно-цифровой код произвольной длины.</summary>
    Code128,
    /// <summary>Двумерный stacked-код PDF417.</summary>
    Pdf417
}
""",
("file", "QRLab.Core/Models/Generation/GenerationDefaults.cs"),
"""using QRLab.Core.Models.Generation.Barcode;
using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Значения параметров генерации по умолчанию. Единый источник истины для
/// конструктора записей GenerationOptions, UI-форм и тестов сверки значений.
/// </summary>
public static class GenerationDefaults
{
    /// <summary>Шаблон содержимого по умолчанию.</summary>
    public const string Template = "LAB-2026-{num}";
    /// <summary>Плейсхолдер номера внутри шаблона.</summary>
    public const string NumberPlaceholder = "{num}";
    /// <summary>Формат номера: четыре цифры с ведущими нулями.</summary>
    public const string NumberFormat = "0000";
    /// <summary>Номер первого образца.</summary>
    public const int FirstNumber = 1;
    /// <summary>Размер пакета по умолчанию.</summary>
    public const int Count = 120;
    /// <summary>Количество образцов в режиме «один код».</summary>
    public const int SingleCount = 1;
    /// <summary>Ширина изображения по умолчанию, px.</summary>
    public const int Width = 360;
    /// <summary>Высота изображения по умолчанию, px.</summary>
    public const int Height = 360;
    /// <summary>Тихая зона по умолчанию, модули.</summary>
    public const int QuietZone = 2;
    /// <summary>0 означает автоматический выбор версии QR.</summary>
    public const int AutomaticQrVersion = 0;
    /// <summary>Уровень коррекции по умолчанию (H — устойчив к искажениям).</summary>
    public const QrCorrectionLevel Correction = QrCorrectionLevel.High;
    /// <summary>Вид штрихкода по умолчанию.</summary>
    public const BarcodeType Barcode = BarcodeType.Code128;
    /// <summary>Режим генерации по умолчанию.</summary>
    public const GenerationMode Mode = GenerationMode.Batch;
    /// <summary>Вид кода по умолчанию.</summary>
    public const CodeKind Kind = CodeKind.QrCode;
    /// <summary>Источник содержимого по умолчанию.</summary>
    public const ContentSource Source = ContentSource.NumberedTemplate;
    /// <summary>Имя нового набора, если пользователь не загрузил сохранённый.</summary>
    public const string NewSetName = "Набор";
}
""",
("file", "QRLab.Core/Models/Generation/GenerationLimits.cs"),
"""namespace QRLab.Core.Models.Generation;

/// <summary>
/// Допустимые границы числовых параметров генерации. Используются валидацией
/// GenerationOptions, ограничителями полей ввода UI и классом RasterImage.
/// </summary>
public static class GenerationLimits
{
    /// <summary>Минимальное количество образцов в пакете.</summary>
    public const int MinimumCount = 1;
    /// <summary>Максимальное количество образцов в пакете.</summary>
    public const int MaximumCount = 1000;
    /// <summary>Максимальный первый номер (чтобы нумерация не переполняла int).</summary>
    public const int MaximumFirstNumber = 999999;
    /// <summary>Минимальный размер стороны изображения, px.</summary>
    public const int MinimumImageSize = 128;
    /// <summary>Максимальный размер стороны изображения, px.</summary>
    public const int MaximumImageSize = 8192;
    /// <summary>Минимальная тихая зона, модули.</summary>
    public const int MinimumQuietZone = 0;
    /// <summary>Максимальная тихая зона, модули.</summary>
    public const int MaximumQuietZone = 16;
    /// <summary>Максимальная версия QR-кода по стандарту.</summary>
    public const int MaximumQrVersion = 40;
}
""",
("file", "QRLab.Core/Models/Generation/GenerationPresets.cs"),
"""using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Встроенная «библиотека наборов» — готовые пресеты, которые пользователь
/// может применить одним кликом на экране генерации.
/// </summary>
public static class GenerationPresets
{
    /// <summary>Список доступных пресетов.</summary>
    public static IReadOnlyList<LibrarySet> Library { get; } =
        Array.AsReadOnly<LibrarySet>([
        new("Тестовый набор", new QrGenerationOptions { Count = 12, Template = "TEST-{num}" }),
    ]);
}
""",
("file", "QRLab.Core/Models/Generation/Barcode/BarcodeGenerationOptions.cs"),
"""namespace QRLab.Core.Models.Generation.Barcode;

/// <summary>
/// Базовая запись для всех штрихкодов. Хранит конкретный вид кода и предоставляет
/// фабричный метод Create, сопоставляющий enum BarcodeType с нужным подтипом опций.
/// </summary>
public abstract record BarcodeGenerationOptions : GenerationOptions
{
    /// <summary>Все штрихкоды относятся к роду Barcode.</summary>
    public override CodeKind Kind => CodeKind.Barcode;
    /// <summary>Общий префикс имён образцов для штрихкодов.</summary>
    public override string SamplePrefix => "BAR";
    /// <summary>Конкретный вид штрихкода (EAN-13, Code 128, PDF417).</summary>
    public abstract BarcodeType BarcodeType { get; }

    /// <summary>Фабрика: создаёт набор параметров по выбранному виду штрихкода.</summary>
    public static BarcodeGenerationOptions Create(BarcodeType type) => type switch
    {
        BarcodeType.Ean13 => new Ean13GenerationOptions(),
        BarcodeType.Code128 => new Code128GenerationOptions(),
        BarcodeType.Pdf417 => new Pdf417GenerationOptions(),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
""",
("file", "QRLab.Core/Models/Generation/Barcode/LinearBarcodeGenerationOptions.cs"),
"""using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Промежуточная запись для линейных (одномерных) штрихкодов: включает флаг
/// IsLinear, из-за которого рендер растягивает код по высоте, а не сохраняет квадрат.
/// PDF417 остаётся «нелинейным» и масштабируется как QR.
/// </summary>
public abstract record LinearBarcodeGenerationOptions : BarcodeGenerationOptions
{
    /// <summary>Признак линейного кода для renderer'а и подсказок ZXing.</summary>
    public override bool IsLinear => true;
}
""",
("file", "QRLab.Core/Models/Generation/Barcode/Code128GenerationOptions.cs"),
"""using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// <summary>Параметры генерации линейного штрихкода Code 128.</summary>
public sealed record Code128GenerationOptions : LinearBarcodeGenerationOptions
{
    /// <summary>Вид кода — Code 128.</summary>
    public override BarcodeType BarcodeType => BarcodeType.Code128;
}
""",
("file", "QRLab.Core/Models/Generation/Barcode/Pdf417GenerationOptions.cs"),
"""using QRLab.Core.Models.Generation.Barcode;

namespace QRLab.Core.Models.Generation;

/// <summary>
/// Параметры генерации двумерного stacked-кода PDF417.
/// Не является линейным: изображение масштабируется пропорционально, как у QR.
/// </summary>
public sealed record Pdf417GenerationOptions : BarcodeGenerationOptions
{
    /// <summary>Вид кода — PDF417.</summary>
    public override BarcodeType BarcodeType => BarcodeType.Pdf417;
}
""",
])

# ==================== QRLab.Core: QR/EAN + Mutations + Samples + Storage ====================
run([
("file", "QRLab.Core/Models/Generation/Qrcode/QrGenerationOptions.cs"),
"""namespace QRLab.Core.Models.Generation.Qrcode;

/// <summary>Параметры генерации QR-кода: версия и уровень коррекции поверх общих настроек.</summary>
public sealed record QrGenerationOptions : GenerationOptions
{
    /// <summary>Версия QR (1..40); 0 — автоматический выбор по объёму данных.</summary>
    public int Version { get; init; } = GenerationDefaults.AutomaticQrVersion;
    /// <summary>Уровень коррекции ошибок L/M/Q/H.</summary>
    public QrCorrectionLevel Correction { get; init; } = GenerationDefaults.Correction;
    /// <summary>Род кода — QR.</summary>
    public override CodeKind Kind => CodeKind.QrCode;
    /// <summary>Префикс имён образцов.</summary>
    public override string SamplePrefix => "QR";

    /// <summary>Проверяет общие параметры и специфичные для QR (версия, коррекция).</summary>
    public override void Validate()
    {
        base.Validate();
        if (Version < GenerationDefaults.AutomaticQrVersion || Version > GenerationLimits.MaximumQrVersion)
            throw new ArgumentException($"Класс размера QR должен быть от 1 до {GenerationLimits.MaximumQrVersion} (0 — авто).");
        if (!Enum.IsDefined(Correction)) throw new ArgumentException("Неизвестный уровень коррекции QR.");
    }
}
""",
("file", "QRLab.Core/Models/Generation/Barcode/Ean13GenerationOptions.cs"),
"""namespace QRLab.Core.Models.Generation.Barcode;

/// <summary>
/// Параметры генерации штрихкода EAN-13. Переопределяет PrepareContent:
/// в пакетном режиме номер дополняется нулями до 12 цифр (контрольную цифру
/// ZXing вычисляет сам), а в текстовом режиме содержимое проверяется на
/// «чистые» ASCII-цифры нужной длины — иначе EAN-кодировщик ZXing принимает
/// и русские буквы, и полноширинные цифры, выдавая некорректный код.
/// </summary>
public sealed record Ean13GenerationOptions : LinearBarcodeGenerationOptions
{
    /// <summary>Вид кода — EAN-13.</summary>
    public override BarcodeType BarcodeType => BarcodeType.Ean13;

    /// <summary>Допустимая длина данных EAN-13: 12 или 13 цифр.</summary>
    private const int DigitsLength = 13;

    /// <summary>
    /// Готовит содержимое: для шаблона с номером возвращает 12 цифр,
    /// для произвольного текста — предварительно проверенную строку.
    /// </summary>
    public override string PrepareContent(int number) => Source == ContentSource.NumberedTemplate
        ? number.ToString("D12", System.Globalization.CultureInfo.InvariantCulture)
        : ValidateText(Template);

    /// <summary>Проверяет, что текст — ровно 12 или 13 обычных ASCII-цифр.</summary>
    private static string ValidateText(string text)
    {
        if (text.Length is not (DigitsLength - 1 or DigitsLength) || !text.All(char.IsAsciiDigit))
            throw new ArgumentException("Содержимое EAN-13 должно содержать 12 или 13 арабских цифр.");
        return text;
    }
}
""",
("file", "QRLab.Core/Models/Mutations/GaussianBlurOptions.cs"),
"""namespace QRLab.Core.Models.Mutations;

/// <summary>Параметры гауссова размытия: одна величина sigma (в пикселях).</summary>
public sealed record GaussianBlurOptions : MutationOptions
{
    /// <summary>Вид мутации — размытие.</summary>
    public override MutationKind Kind => MutationKind.GaussianBlur;

    /// <summary>Стандартное отклонение гауссова ядра; 0 означает «без эффекта».</summary>
    public double Sigma { get; init; } = MutationDefaults.Sigma;

    /// <summary>Проверяет конечность и диапазон sigma.</summary>
    public override void Validate()
    {
        if (!double.IsFinite(Sigma) || Sigma < MutationDefaults.MinimumSigma || Sigma > MutationDefaults.MaximumSigma)
            throw new ArgumentException($"Сила размытия должна быть от 0 до {MutationDefaults.MaximumSigma}.");
    }
}
""",
("file", "QRLab.Core/Models/Mutations/MutationDefaults.cs"),
"""namespace QRLab.Core.Models.Mutations;

/// <summary>Значения по умолчанию и границы параметров всех мутаций.</summary>
public static class MutationDefaults
{
    /// <summary>Sigma размытия по умолчанию, px.</summary>
    public const double Sigma = 1.2;
    /// <summary>Минимальная sigma (0 — эффект не применяется).</summary>
    public const double MinimumSigma = 0;
    /// <summary>Максимальная sigma.</summary>
    public const double MaximumSigma = 50;
    /// <summary>Угол наклона перспективы вокруг X по умолчанию, градусы.</summary>
    public const double AngleX = 8;
    /// <summary>Угол наклона перспективы вокруг Y по умолчанию, градусы.</summary>
    public const double AngleY = -4;
    /// <summary>Максимальный модуль угла перспективы.</summary>
    public const double MaximumAngle = 60;
    /// <summary>Относительное расстояние камеры по умолчанию.</summary>
    public const double Distance = 2;
    /// <summary>Минимальное расстояние камеры.</summary>
    public const double MinimumDistance = 1;
    /// <summary>Максимальное расстояние камеры.</summary>
    public const double MaximumDistance = 10;
    /// <summary>Плотность шума «соль и перец» по умолчанию, % пикселей.</summary>
    public const double NoisePercent = 1;
    /// <summary>Доля белых точек среди зашумлённых пикселей по умолчанию, %.</summary>
    public const double SaltPercent = 50;
    /// <summary>Зерно генератора случайных чисел по умолчанию (детерминизм).</summary>
    public const int Seed = 42;
}
""",
("file", "QRLab.Core/Models/Mutations/MutationFrame.cs"),
"""using QRLab.Core.Models.Images;

namespace QRLab.Core.Models.Mutations;

/// <summary>Кадр цепочки мутаций: применённые параметры и полученное изображение.</summary>
public sealed record MutationFrame(MutationOptions Options, RasterImage Image);
""",
("file", "QRLab.Core/Models/Mutations/MutationKind.cs"),
"""namespace QRLab.Core.Models.Mutations;

/// <summary>Виды поддерживаемых искажений изображений.</summary>
public enum MutationKind { GaussianBlur, SaltAndPepper, Perspective }
""",
("file", "QRLab.Core/Models/Mutations/PerspectiveMutationOptions.cs"),
"""namespace QRLab.Core.Models.Mutations;

/// <summary>Способ интерполяции при перспективной пересэмплинг картинки.</summary>
public enum ImageInterpolation { Bicubic, Bilinear, Nearest }

/// <summary>
/// Параметры перспективного искажения: два угла поворота «плоскости» изображения
/// и расстояние виртуальной камеры (чем меньше — тем сильнее трапеция).
/// </summary>
public sealed record PerspectiveMutationOptions : MutationOptions
{
    /// <summary>Вид мутации — перспектива.</summary>
    public override MutationKind Kind => MutationKind.Perspective;
    /// <summary>Угол поворота вокруг оси X, градусы (-60..60).</summary>
    public double AngleX { get; init; } = MutationDefaults.AngleX;
    /// <summary>Угол поворота вокруг оси Y, градусы (-60..60).</summary>
    public double AngleY { get; init; } = MutationDefaults.AngleY;
    /// <summary>Расстояние камеры (множитель фокусного), 1..10.</summary>
    public double Distance { get; init; } = MutationDefaults.Distance;
    /// <summary>Фильтр интерполяции при обратном проходе лучей.</summary>
    public ImageInterpolation Interpolation { get; init; } = ImageInterpolation.Bicubic;

    /// <summary>Проверяет конечность углей, их диапазон, расстояние и интерполяцию.</summary>
    public override void Validate()
    {
        if (!double.IsFinite(AngleX) || Math.Abs(AngleX) > MutationDefaults.MaximumAngle
            || !double.IsFinite(AngleY) || Math.Abs(AngleY) > MutationDefaults.MaximumAngle
            || !double.IsFinite(Distance) || Distance < MutationDefaults.MinimumDistance || Distance > MutationDefaults.MaximumDistance
            || !Enum.IsDefined(Interpolation))
            throw new ArgumentException("Недопустимые параметры перспективы.");
    }
}
""",
("file", "QRLab.Core/Models/Mutations/SaltAndPepperOptions.cs"),
"""namespace QRLab.Core.Models.Mutations;

/// <summary>
/// Параметры импульсного шума «соль и перец»: какой доля пикселей управлять
/// и какая часть из них станет белой («соль») вместо чёрного («перец»).
/// Seed задаёт детерминированность: те же параметры — тот же рисунок шума.
/// </summary>
public sealed record SaltAndPepperOptions : MutationOptions
{
    /// <summary>Вид мутации — импульсный шум.</summary>
    public override MutationKind Kind => MutationKind.SaltAndPepper;
    /// <summary>Плотность шума: % пикселей, которые будут заменены, 0..100.</summary>
    public double DensityPercent { get; init; } = MutationDefaults.NoisePercent;
    /// <summary>Доля белых точек среди зашумлённых пикселей, %, 0..100.</summary>
    public double SaltPercent { get; init; } = MutationDefaults.SaltPercent;
    /// <summary>Зерно ГСЧ для воспроизводимости результата.</summary>
    public int Seed { get; init; } = MutationDefaults.Seed;

    /// <summary>Проверяет конечность и процентные диапазоны обоих параметров.</summary>
    public override void Validate()
    {
        if (!double.IsFinite(DensityPercent) || DensityPercent is < 0 or > 100
            || !double.IsFinite(SaltPercent) || SaltPercent is < 0 or > 100)
            throw new ArgumentException("Плотность шума и доля белых точек должны быть от 0 до 100%.");
    }
}
""",
("file", "QRLab.Core/Models/Samples/SamplePreview.cs"),
"""using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;

namespace QRLab.Core.Models.Samples;

/// <summary>
/// Полное описание одного образца: номер, имя, закодированное содержимое,
/// исходное PNG-изображение, кадры мутаций и настройки, которыми он создан.
/// Это центральная сквозная модель между генерацией, редактированием,
/// распознаванием и хранилищем.
/// </summary>
public sealed record SamplePreview(
    int Number,
    string Name,
    string Content,
    RasterImage Image,
    IReadOnlyList<MutationFrame> Frames,
    GenerationOptions Settings
    );
""",
])

# ==================== QRLab.Core: Recognition + Storage ====================
run([
("file", "QRLab.Core/Models/Recognition/RecognitionDefaults.cs"),
"""namespace QRLab.Core.Models.Recognition;

/// <summary>Значения по умолчанию и границы для распознавания кодов.</summary>
public static class RecognitionDefaults
{
    /// <summary>Название декодера, попадает в отчёты/интерфейс.</summary>
    public const string DecoderName = "ZXing.Net";
    /// <summary>Тайм-аут декодирования одного изображения по умолчанию, мс.</summary>
    public const int TimeoutMilliseconds = 500;
    /// <summary>Минимально допустимый тайм-аут, мс.</summary>
    public const int MinimumTimeoutMilliseconds = 1;
    /// <summary>Максимально допустимый тайм-аут, мс.</summary>
    public const int MaximumTimeoutMilliseconds = 60000;
    /// <summary>«Улучшенный» режим (TryHarder) по умолчанию.</summary>
    public const bool Enhanced = true;
}
""",
("file", "QRLab.Core/Models/Recognition/RecognitionOptions.cs"),
"""namespace QRLab.Core.Models.Recognition;

/// <summary>Параметры одного прогона распознавания: лимит времени и режим качества.</summary>
public sealed record RecognitionOptions(
    int TimeoutMilliseconds = RecognitionDefaults.TimeoutMilliseconds,
    bool Enhanced = RecognitionDefaults.Enhanced)
{
    /// <summary>Проверяет, что тайм-аут лежит в допустимом диапазоне.</summary>
    public void Validate()
    {
        if (TimeoutMilliseconds < RecognitionDefaults.MinimumTimeoutMilliseconds || TimeoutMilliseconds > RecognitionDefaults.MaximumTimeoutMilliseconds)
            throw new ArgumentException($"Тайм-аут должен быть от {RecognitionDefaults.MinimumTimeoutMilliseconds} до {RecognitionDefaults.MaximumTimeoutMilliseconds} мс.");
    }
}
""",
("file", "QRLab.Core/Models/Recognition/RecognitionPreview.cs"),
"""namespace QRLab.Core.Models.Recognition;

/// <summary>
/// Результат распознавания одного образца: номер, итоговый статус,
/// длительность декодирования, считанный текст и сообщение об ошибке.
/// </summary>
public sealed record RecognitionPreview(int Number, Status Status, double Milliseconds, string? DecodedContent = null, string? Error = null);
""",
("file", "QRLab.Core/Models/Recognition/RecognitionReportRow.cs"),
"""namespace QRLab.Core.Models.Recognition;

/// <summary>
/// Одна строка CSV-отчёта о распознавании: ожидаемое и фактически считанное
/// содержимое, статус, время декодирования, цепочка мутаций и ошибка.
/// </summary>
public sealed record RecognitionReportRow(int Number, string Name, string Format, string ExpectedContent,
    string? DecodedContent, Status? Status, double? Milliseconds, string MutationChain, string? Error);
""",
("file", "QRLab.Core/Models/Recognition/Status.cs"),
"""namespace QRLab.Core.Models.Recognition;

/// <summary>Итоговые состояния распознавания одного изображения.</summary>
public enum Status
{
    /// <summary>Код считан и совпадает с ожидаемым содержимым.</summary>
    Success,
    /// <summary>Код не удалось найти/декодировать.</summary>
    Failed,
    /// <summary>Декодирование не уложилось в тайм-аут.</summary>
    Timeout,
    /// <summary>Код считан, но текст не совпадает с ожидаемым.</summary>
    Mismatch
}
""",
("file", "QRLab.Core/Models/Storage/ExportOptions.cs"),
"""namespace QRLab.Core.Models.Storage;

/// <summary>Параметры экспорта набора: целевая папка на диске.</summary>
public sealed record ExportOptions(string Destination);
""",
("file", "QRLab.Core/Models/Storage/SampleSet.cs"),
"""using QRLab.Core.Models.Samples;

namespace QRLab.Core.Models.Storage;

/// <summary>Сохраняемый/загружаемый набор: имя и все образцы с их состоянием.</summary>
public sealed record SampleSet(string Name, IReadOnlyList<SamplePreview> Samples);
""",
])
