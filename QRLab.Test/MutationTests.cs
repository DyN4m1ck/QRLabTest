using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.Image.Mutations;
using QRLab.Test.Fixtures;
using SkiaSharp;
using Xunit;

namespace QRLab.Test;

public sealed class MutationTests
{
    [Fact]
    public void GaussianBlurSpreadsAnImpulseSymmetricallyWithoutChangingOriginal()
    {
        var original = Impulse();
        var before = original.Data.ToArray();
        var result = new GaussianBlurMutation().Apply(original, new GaussianBlurOptions { Sigma = 2 });
        Assert.Equal(before, original.Data.ToArray());
        Assert.Equal(original.Width, result.Width); Assert.Equal(original.Height, result.Height);
        Assert.False(before.AsSpan().SequenceEqual(result.Data.Span));
        using var stream = result.OpenRead();
        using var pixels = SKBitmap.Decode(stream);
        var center = pixels.GetPixel(32, 32);
        var left = pixels.GetPixel(31, 32);
        var right = pixels.GetPixel(33, 32);
        Assert.InRange(center.Red, (byte)1, (byte)254);
        Assert.True(center.Red <= left.Red);
        Assert.Equal(left, right);
        Assert.Equal(left, pixels.GetPixel(32, 31));
        Assert.Equal(SKColors.White, pixels.GetPixel(0, 0));
        Assert.Equal(255, center.Alpha);
    }

    [Fact]
    public void PlaceholderMutationsAndZeroBlurPreserveExactImageInstance()
    {
        var original = Impulse();
        Assert.Same(original, new GaussianBlurMutation().Apply(original, new GaussianBlurOptions { Sigma = 0 }));
        var frames = TestApplicationFactory.CreateMutations().Apply(original,
            [new InkMutationOptions(), new PerspectiveMutationOptions()]);
        Assert.All(frames, frame => Assert.Same(original, frame.Image));
    }

    [Fact]
    public void PipelineAppliesEachMutationToPreviousResult()
    {
        var original = Impulse();
        var frames = TestApplicationFactory.CreateMutations().Apply(original,
            [new GaussianBlurOptions { Sigma = 2 }, new InkMutationOptions(), new GaussianBlurOptions { Sigma = 2 }, new PerspectiveMutationOptions()]);
        Assert.Same(frames[0].Image, frames[1].Image);
        Assert.Same(frames[2].Image, frames[3].Image);
        using var firstStream = frames[0].Image.OpenRead();
        using var secondStream = frames[2].Image.OpenRead();
        using var first = SKBitmap.Decode(firstStream);
        using var second = SKBitmap.Decode(secondStream);
        Assert.True(second.GetPixel(32, 32).Red > first.GetPixel(32, 32).Red);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(51)]
    public void GaussianBlurRejectsInvalidSigma(double sigma) =>
        Assert.Throws<ArgumentException>(() => new GaussianBlurMutation().Apply(Impulse(), new GaussianBlurOptions { Sigma = sigma }));

    [Fact]
    public void EditingEarlierFrameRebuildsDownstreamFramesAndResetKeepsOriginal()
    {
        var vm = TestApplicationFactory.CreateMainWindow();
        vm.Generation.Mode = QRLab.Core.Models.Generation.GenerationMode.Single;
        vm.Generation.ImageWidth = 256; vm.Generation.ImageHeight = 256;
        vm.Generation.GenerateCommand.Execute(null);
        var sample = vm.Workspace.SelectedSample!;
        var sourceBytes = sample.OriginalImage.Data.ToArray();
        var editor = vm.Mutations;
        editor.AddCommand.Execute(MutationKind.GaussianBlur);
        editor.AddCommand.Execute(MutationKind.Ink);
        var first = sample.Image;
        editor.SelectedOperation = editor.Operations[0];
        editor.Blur = 4;
        Assert.NotSame(first, sample.Image);
        Assert.Same(editor.Operations[0].Image, editor.Operations[1].Image);
        Assert.Same(editor.Operations[0].Image, editor.PreviewImage);
        editor.ResetCommand.Execute(null);
        Assert.Same(sample.OriginalImage, sample.Image);
        Assert.Equal(sourceBytes, sample.OriginalImage.Data.ToArray());
    }

    private static RasterImage Impulse()
    {
        using var surface = SKSurface.Create(new SKImageInfo(65, 65, SKColorType.Bgra8888, SKAlphaType.Opaque));
        surface.Canvas.Clear(SKColors.White);
        using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = false };
        surface.Canvas.DrawRect(32, 32, 1, 1, paint);
        using var image = surface.Snapshot();
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);
        return new RasterImage(png.AsSpan());
    }
}