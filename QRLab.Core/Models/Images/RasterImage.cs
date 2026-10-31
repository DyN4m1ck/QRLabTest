using System.Buffers.Binary;
using QRLab.Core.Models.Generation;

namespace QRLab.Core.Models.Images;

/// <summary>
/// Неизменяемый (immutable) буфер PNG-изображения — единый формат «картинки»,
/// который передаётся между генерацией, мутациями, предпросмотром и хранилищем.
/// Байты копируются внутрь объекта, поэтому изменить изображение извне нельзя;
/// размеры считываются прямо из заголовка IHDR при создании экземпляра.
/// </summary>
public sealed class RasterImage
{
    /// <summary>Название формата — используется в интерфейсе и описаниях наборов.</summary>
    public const string FormatName = "PNG";
    /// <summary>Расширение файла при сохранении на диск.</summary>
    public const string FileExtension = ".png";

    /// <summary>Внутренний массив байтов PNG; наружу отдаётся только как ReadOnlyMemory.</summary>
    private readonly byte[] _data;

    /// <summary>Ширина изображения в пикселях (из chunk IHDR).</summary>
    public int Width { get; }
    /// <summary>Высота изображения в пикселях (из chunk IHDR).</summary>
    public int Height { get; }

    /// <summary>Байты PNG для чтения без лишнего копирования (SkiaSharp, ZXing и т.п.).</summary>
    public ReadOnlyMemory<byte> Data => _data;

    /// <summary>
    /// Создаёт изображение из байтов PNG: проверяет сигнатуру файла и наличие
    /// chunk IHDR, затем считывает размеры и убеждается, что они в допустимых пределах.
    /// </summary>
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

    /// <summary>Открывает поток для чтения байтов (только для чтения).</summary>
    public Stream OpenRead() => new MemoryStream(_data, writable: false);
    /// <summary>Записывает байты PNG в произвольный поток (используется хранилищем).</summary>
    public void WriteTo(Stream destination) => destination.Write(_data);
}
