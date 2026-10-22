using QRLab.Core.Models;
using QRLab.View.ViewModels;
using Xunit;

namespace QRLab.Test;

public sealed class ModelTests
{
    [Fact]
    public void ProjectHasIdentityAndIndependentSampleCollection()
    {
        var project = new ExperimentProject();
        var other = new ExperimentProject();
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.NotEqual(project.Id, other.Id);
        Assert.Equal("Новый проект", project.Name);
        project.Samples.Add(new CodeSample());
        Assert.Single(project.Samples);
        Assert.Empty(other.Samples);
    }

    [Fact]
    public void SampleRetainsPayloadAndMutationDescription()
    {
        var sample = new CodeSample { Name = "Образец", Content = "QRLab", Format = Format.QrCode };
        var mutation = new MutationStep { Name = "Шум", Intensity = 0.25 };
        sample.Mutations.Add(mutation);
        Assert.NotEqual(Guid.Empty, sample.Id);
        Assert.NotEqual(Guid.Empty, mutation.Id);
        Assert.Equal("QRLab", sample.Content);
        Assert.Equal(Format.QrCode, sample.Format);
        Assert.Equal(0.25, Assert.Single(sample.Mutations).Intensity);
    }

    [Fact]
    public void DecodeResultRetainsSampleAndOutcome()
    {
        var sample = new CodeSample();
        var result = new DecodeResult
        {
            SampleId = sample.Id, IsSuccess = true,
            DecodedText = "QRLab", Duration = TimeSpan.FromMilliseconds(15)
        };
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(sample.Id, result.SampleId);
        Assert.True(result.IsSuccess);
        Assert.Equal("QRLab", result.DecodedText);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(TimeSpan.FromMilliseconds(15), result.Duration);
        Assert.False(new DecodeResult().IsSuccess);
    }

    [Fact]
    public void NavigationRaisesNotificationsForAllSections()
    {
        var vm = new MainWindowViewModel(new Image.Demo.PrototypeData());
        Assert.Equal(4, vm.Screens.Count);
        Assert.Equal("Генерация кодов", vm.SelectedScreen.Title);
        var notifications = new List<string?>();
        vm.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);
        foreach (var screen in vm.Screens.Skip(1))
        {
            vm.SelectedScreen = screen;
            Assert.Same(screen, vm.SelectedScreen);
        }
        Assert.Equal(3, notifications.Count(n => n == nameof(vm.SelectedScreen)));
    }
}
