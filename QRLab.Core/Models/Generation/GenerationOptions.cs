using System.Globalization;
using System.Text.Json.Serialization;
using QRLab.Core.Models.Generation.Qrcode;

namespace QRLab.Core.Models.Generation;

/// Базовая запись всех параметров генерации кода.
/// Помечена как полиморфная для System.Text.Json: при сохранении набора в манифест
/// пишется дискриминатор "$code" ("qr", "ean13", "code128", "pdf417"), что позволяет
/// восстановить конкретный подтип без рефлексии и безопасно при trimming.
/// Наследники добавляют специфичные для формата настройки и переопределяют
/// <see cref="PrepareContent"/> и <see cref="Validate"/>.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$code")]
[JsonDerivedType(typeof(QrGenerationOptions), "qr")]
[JsonDerivedType(typeof(Ean13GenerationOptions), "ean13")]
[JsonDerivedType(typeof(Code128GenerationOptions), "code128")]
[JsonDerivedType(typeof(Pdf417GenerationOptions), "pdf417")]
public abstract record GenerationOptions
{
    /// Род вида кода (QR или штрихкод); используется для выбора кодировщика.
    [JsonIgnore] public abstract CodeKind Kind { get; }
    /// Линейный ли код; влияет на масштабирование при рендере и отступы.
    [JsonIgnore] public virtual bool IsLinear => false;
    /// Префикс имени образца ("QR", "BAR" и т.п.).
    [JsonIgnore] public abstract string SamplePrefix { get; }

    /// Шаблон содержимого (например "LAB-2026-{num}") либо готовый текст.
    public string Template { get; init; } = GenerationDefaults.Template;
    /// Номер первого образца в пакете.
    public int FirstNumber { get; init; } = GenerationDefaults.FirstNumber;
    /// Сколько образцов сгенерировать.
    public int Count { get; init; } = GenerationDefaults.Count;
    /// Ширина итогового PNG в пикселях.
    public int Width { get; init; } = GenerationDefaults.Width;
    /// Высота итогового PNG в пикселях.
    public int Height { get; init; } = GenerationDefaults.Height;
    /// Тихая зона (отступ) вокруг кода в модулях.
    public int QuietZone { get; init; } = GenerationDefaults.QuietZone;
    /// Источник содержимого: шаблон с номером или произвольный текст.
    public ContentSource Source { get; init; } = GenerationDefaults.Source;

    /// Проверка корректности параметров перед генерацией. Бросает
    /// <see cref="ArgumentException"/> с русским сообщением, понятным пользователю UI.
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

    /// Готовит содержимое i-го образца: в режиме шаблона подставляет номер
    /// с фиксированной шириной ("0000"), в режиме текста возвращает шаблон как есть.
    public virtual string PrepareContent(int number) => Source == ContentSource.NumberedTemplate
        ? Template.Replace(GenerationDefaults.NumberPlaceholder, number.ToString(GenerationDefaults.NumberFormat, CultureInfo.InvariantCulture))
        : Template;

    /// Формирует имя файла/образца по его номеру, например "QR-0012".
    public string GetSampleName(int number) => $"{SamplePrefix}-{number.ToString(GenerationDefaults.NumberFormat, CultureInfo.InvariantCulture)}";
}
