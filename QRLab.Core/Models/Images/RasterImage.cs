using System.Buffers.Binary;
using QRLab.Core.Models.Generation;

namespace QRLab.Core.Models.Images;

/// <summary>Immutable PNG data shared by generation, mutation, previews and storage.</summary>
public sealed class RasterImage
{
    public const string FormatName = "PNG";
    public const string FileExtension = ".png";
    
    private readonly byte[] _data;
    
    public int Width { get; }
    public int Height { get; }
    
    public ReadOnlyMemory<byte> Data => _data;

    public RasterImage(ReadOnlySpan<byte> data)
    {
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (data.Length < 33 || !data[..8].SequenceEqual(signature)
            || !data.Slice(12, 4).SequenceEqual("IHDR"u8))
            throw new ArgumentException("Некорректное PNG-изображение.");
        
        var width = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(16, 4));
        var height = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(20, 4));
        if (width is < 1 or > GenerationLimits.MaximumImageSize || height is < 1 or > GenerationLimits.MaximumImageSize)
            throw new ArgumentException("Недопустимый размер PNG-изображения.");
        
        Width = (int)width;
        Height = (int)height;
        _data = data.ToArray();
    }

    public Stream OpenRead() => new MemoryStream(_data, writable: false);
    public void WriteTo(Stream destination) => destination.Write(_data);
}