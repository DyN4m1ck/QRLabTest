using QRLab.Core.Models.Generation;
using QRLab.Core.Models.Generation.Barcode;
using QRLab.Core.Models.Generation.Qrcode;
using QRLab.Core.Models.Images;
using QRLab.Core.Models.Mutations;
using QRLab.Core.Models.Recognition;
using QRLab.Image.Generation;
using QRLab.Image.Recognition;
using QRLab.Test.Fixtures;
using QRLab.View.Presentation.Recognition;
using Xunit;

namespace QRLab.Test;

public sealed class RecognitionTests
{
    [Theory]
    [InlineData(CodeKind.QrCode, BarcodeType.Code128, "Привет, ZXing")]
    [InlineData(CodeKind.Barcode, BarcodeType.Ean13, "590123412345")]
    [InlineData(CodeKind.Barcode, BarcodeType.Code128, "LAB-012345")]
    [InlineData(CodeKind.Barcode, BarcodeType.Pdf417, "PDF417 текст")]
    public async Task ReadsActualPngForEachSupportedCode(CodeKind kind, BarcodeType type, string content)
    {
        GenerationOptions settings = kind == CodeKind.QrCode ? new QrGenerationOptions() : BarcodeGenerationOptions.Create(type);
        var sample = Assert.Single(new CodeGenerator().CreateSamples(settings with { Template = content, Count = 1, Width = 768, Height = 512 }));
        var result = Assert.Single(await new ZxingCodeRecognizer().RecognizeAsync([sample], new(10000), TestContext.Current.CancellationToken));
        Assert.Equal(Status.Success, result.Status);
        Assert.Equal(sample.Content, result.DecodedContent);
        Assert.True(result.Milliseconds >= 0);
    }

    [Fact]
    public async Task ReadsLastMutationInsteadOfOriginalOrMetadata()
    {
        var sample = Assert.Single(new CodeGenerator().CreateSamples(new QrGenerationOptions { Template = "actual", Count = 1, Width = 256, Height = 256 }));
        var recognizer = new ZxingCodeRecognizer();
        var mismatch = Assert.Single(await recognizer.RecognizeAsync([sample with { Content = "different" }], new(10000), TestContext.Current.CancellationToken));
        Assert.Equal(Status.Mismatch, mismatch.Status);
        Assert.Equal("actual", mismatch.DecodedContent);
        var frames = TestApplicationFactory.CreateMutations().Apply(sample.Image,
            [new SaltAndPepperOptions { DensityPercent = 100, SaltPercent = 0 }]);
        var failure = Assert.Single(await recognizer.RecognizeAsync([sample with { Frames = frames }], new(10000), TestContext.Current.CancellationToken));
        Assert.Equal(Status.Failed, failure.Status);
        Assert.Null(failure.DecodedContent);
    }

    [Fact]
    public async Task CancellationAndTimeoutAreDistinctFromFailedDecode()
    {
        var sample = Assert.Single(new CodeGenerator().CreateSamples(new QrGenerationOptions { Count = 1, Width = 4096, Height = 4096 }));
        var recognizer = new ZxingCodeRecognizer();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => recognizer.RecognizeAsync([sample], new(), cancellation.Token));
        var result = Assert.Single(await recognizer.RecognizeAsync([sample], new(1), TestContext.Current.CancellationToken));
        Assert.Equal(Status.Timeout, result.Status);
    }

    [Fact]
    public async Task ReadingFiltersActualResultsAndInvalidatesThemAfterMutation()
    {
        var vm = TestApplicationFactory.CreateMainWindow();
        vm.Generation.Count = 2; vm.Generation.ImageWidth = 256; vm.Generation.ImageHeight = 256;
        vm.Generation.GenerateCommand.Execute(null);
        Assert.Equal(0, vm.Reading.ResultsChecked);
        vm.Reading.Timeout = 10000;
        await vm.Reading.RunCommand.ExecuteAsync(null);
        Assert.Equal(2, vm.Reading.ResultsSuccess);
        Assert.All(vm.Workspace.Samples, sample => Assert.Equal(sample.Content, sample.Result!.DecodedContent));
        vm.Mutations.AddCommand.Execute(MutationKind.GaussianBlur);
        Assert.Equal(1, vm.Reading.ResultsChecked);
        vm.Mutations.NoiseDensity = 100; vm.Mutations.SaltPercent = 0;
        vm.Mutations.AddCommand.Execute(MutationKind.SaltAndPepper);
        await vm.Reading.RunCommand.ExecuteAsync(null);
        Assert.Equal(1, vm.Reading.ResultsSuccess);
        Assert.Equal(1, vm.Reading.ResultsFailed);
        vm.Reading.Filter = RecognitionFilter.Failed;
        Assert.Same(vm.Workspace.SelectedSample, Assert.Single(vm.Reading.Results));
        vm.Reading.Search = "no-match";
        Assert.Empty(vm.Reading.Results);
    }
}