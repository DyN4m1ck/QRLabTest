namespace QRLab.Core.Models;

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
