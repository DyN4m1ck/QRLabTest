namespace QRLab.Core.Models;

/// Устаревшая (legacy) модель образца кода из ранних версий проекта.
/// Сейчас полноценные образцы описываются записью SamplePreview;
/// этот класс оставлен и покрывается только тестом ModelTests.
public sealed class CodeSample
{
    /// Уникальный идентификатор образца.
    public Guid Id { get; init; } = Guid.NewGuid();
    /// Отображаемое имя.
    public string Name { get; set; } = "Новый образец";
    /// Кодируемое содержимое.
    public string Content { get; set; } = string.Empty;
    /// Формат кода (QR или штрихкод).
    public Format Format { get; set; } = Format.QrCode;
    /// Цепочка описаний мутаций (без самих изображений).
    public List<MutationStep> Mutations { get; } = [];
}
