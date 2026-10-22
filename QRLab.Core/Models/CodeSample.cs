namespace QRLab.Core.Models;

public sealed class CodeSample
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Новый образец";
    public string Content { get; set; } = string.Empty;
    public Format Format { get; set; } = Format.QrCode;
    public List<MutationStep> Mutations { get; } = [];
}
