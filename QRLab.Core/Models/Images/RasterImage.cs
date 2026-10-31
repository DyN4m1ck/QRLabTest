using System.Buffers.Binary;
using QRLab.Core.Models.Generation;

namespace QRLab.Core.Models.Images;

/// Неизменяемый (immutable) буфер PNG-изображения — единый формат «картинки»,
/// который передаётся между генерацией, мутациями, предпросмотром и хранилищем.
/// Байты копируются внутрь объекта, поэтому изменить изображение извне нельзя;
/// размеры считываются прямо из заголовка IHDR при создании экземпляра.
public sealed class RasterImage
{
    /// Название формата — используется в интерфейсе и описаниях наборов.
    public const string FormatName = "PNG";
    /// Расширение файла при сохранении на диск.
    public const string FileExtension = ".png";

    /// Внутренний массив байтов PNG; наружу отдаётся только как ReadOnlyMemory.
    private readonly byte[] _data;

    /// Ширина изображения в пикселях (из chunk IHDR).
    public int Width { get; }
    /// Высота изображения в пикселях (из chunk IHDR).
    public int Height { get; }

    /// Байты PNG для чтения без лишнего копирования (SkiaSharp, ZXing и т.п.).
    public ReadOnlyMemory<byte> Data => _data;

    /// Создаёт изображение из байтов PNG: проверяет сигнатуру файла и наличие
    /// chunk IHDR, затем считывает размеры и убеждается, что они в допустимых пределах.
    public RasterImage(ReadOnlySpan<byte> data)
    {
        // Стандартная сигнатура PNG: 89 50 4E 47 0D 0A 1A 0A.
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (data.Length < 33 || !data[..8].SequenceEqual(signature)
            || !data.Slice(12, 4).SequenceEqual("IHDR"u8))
            throw new ArgumentException("Некорректное PNG-изображение.");

        // Ширина и высота хранятся в IHDR как 32-битные числа в big-endian порядке.
        var width = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(16, 4));
        var height = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(20, 4));
        if (width is < 1 or > GenerationLimits.MaximumImageSize || height is < 1 or > GenerationLimits.MaximumImageSize)
            throw new ArgumentException("Недопустимый размер PNG-изображения.");

        Width = (int)width;
        Height = (int)height;
        _data = data.ToArray();
    }

    /// Открывает поток для чтения байтов (только для чтения).
    public Stream OpenRead() => new MemoryStream(_data, writable: false);
    /// Записывает байты PNG в произвольный поток (используется хранилищем).
    public void WriteTo(Stream destination) => destination.Write(_data);
}
