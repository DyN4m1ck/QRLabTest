using QRLab.Core.Models.Mutations;
using QRLab.Test.Fixtures;
using QRLab.Core.Models.Generation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using QRLab.View.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using QRLab.Core.Models.Generation.Barcode;
using QRLab.View;
using QRLab.View.ViewModels;
using QRLab.View.Views;
using Xunit;
[assembly: AvaloniaTestApplication(typeof(QRLab.Test.TestAppBuilder))]
namespace QRLab.Test;
public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().WithInterFont().UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
public sealed class WindowTests
{
    [AvaloniaFact]
    public void LogoMenuControlsBorderlessWindow()
    {
        var window = new MainWindow { DataContext = TestApplicationFactory.CreateMainWindow() };
        try
        {
            window.Show();
            Assert.Equal(WindowDecorations.None, window.WindowDecorations);
            Assert.NotNull(window.Icon);
            var button = window.FindControl<Button>("WindowMenuButton")!;
            var menu = Assert.IsType<MenuFlyout>(button.Flyout);
            menu.ShowAt(button);
            Dispatcher.UIThread.RunJobs();
            var items = menu.Items.OfType<MenuItem>().ToArray();
            Assert.Equal(new[] { "Выход", "Свернуть", "Развернуть" }, items.Select(item => item.Header));
            items[2].RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal(WindowState.Maximized, window.WindowState);
            Assert.Equal("Восстановить", items[2].Header);
            items[2].RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal(WindowState.Normal, window.WindowState);
            Assert.Equal("Развернуть", items[2].Header);
            items[1].RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Assert.Equal(WindowState.Minimized, window.WindowState);
            window.WindowState = WindowState.Normal;
            items[0].RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            Assert.False(window.IsVisible);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void AllThreeScreensLoadAndNavigationChangesVisibleContent()
    {
        var vm = TestApplicationFactory.CreateMainWindow();
        var window = new MainWindow { DataContext = vm };
        try
        {
            window.Show();
            foreach (var screen in vm.Screens)
            {
                var navigation = window.GetVisualDescendants().OfType<ListBox>().Single(l => l.Classes.Contains("nav"));
                navigation.SelectedItem = screen;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Assert.Same(screen, vm.SelectedScreen);
                var navIcons = navigation.GetVisualDescendants().OfType<SvgIcon>().ToArray();
                Assert.Equal(3, navIcons.Length);
                Assert.All(navIcons, icon =>
                {
                    var item = icon.GetVisualAncestors().OfType<ListBoxItem>().First();
                    var expected = Color.Parse(item.IsSelected ? "#75D9BE" : "#86928F");
                    Assert.Equal(expected, Assert.IsAssignableFrom<ISolidColorBrush>(icon.Foreground).Color);
                    var caption = item.GetVisualDescendants().OfType<TextBlock>().Single();
                    Assert.Equal(expected, Assert.IsAssignableFrom<ISolidColorBrush>(caption.Foreground).Color);
                });
                var text = window.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
                Assert.Contains(screen.Title, text);
                Assert.Contains(screen.Description, text);
                Assert.DoesNotContain(text, t => t?.Contains("Функциональность будет добавлена") == true);
                if (screen != vm.Reading) Assert.NotEmpty(window.GetVisualDescendants().OfType<QRLab.View.Controls.RasterPreview>());
                if (Environment.GetEnvironmentVariable("QRLAB_CAPTURE") is { } directory)
                {
                    Directory.CreateDirectory(directory);
                    using var bitmap = new RenderTargetBitmap(new PixelSize(1440, 940), new Vector(96, 96));
                    bitmap.Render(window); using var output = File.Create(Path.Combine(directory, $"screen-{screen.Number}.png")); bitmap.Save(output, PngBitmapEncoderOptions.Default);
                }
            }
        }
        finally { window.Close(); }
    }
    [AvaloniaFact]
    public void GenerationSelectionsBindEnumsInBothDirections()
    {
        var vm = TestApplicationFactory.CreateMainWindow();
        var window = new MainWindow { DataContext = vm };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var formatList = window.GetVisualDescendants().OfType<ListBox>()
                .Single(list => ReferenceEquals(list.ItemsSource, vm.Generation.Formats));
            formatList.SelectedItem = vm.Generation.Formats.Single(option => option.Id == CodeKind.Barcode);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(CodeKind.Barcode, vm.Generation.Format);

            var barcodeCombo = window.GetVisualDescendants().OfType<ComboBox>()
                .Single(combo => ReferenceEquals(combo.ItemsSource, vm.Generation.BarcodeTypes));
            barcodeCombo.SelectedItem = vm.Generation.BarcodeTypes.Single(option => option.Id == BarcodeType.Ean13);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(BarcodeType.Ean13, vm.Generation.BarcodeType);
            Assert.True(vm.Generation.IsEan13);

            vm.Generation.Format = CodeKind.QrCode;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(CodeKind.QrCode, formatList.SelectedValue);

            var sourceCombo = window.GetVisualDescendants().OfType<ComboBox>()
                .Single(combo => ReferenceEquals(combo.ItemsSource, vm.Generation.Sources));
            sourceCombo.SelectedItem = vm.Generation.Sources.Single(option => option.Id == ContentSource.Text);
            Dispatcher.UIThread.RunJobs();
            vm.Generation.Mode = GenerationMode.Single;
            vm.Generation.Template = "Literal-{num}";
            vm.Generation.GenerateCommand.Execute(null);
            Assert.Equal(ContentSource.Text, vm.Generation.Source);
            Assert.Equal("Literal-{num}", Assert.Single(vm.Workspace.Samples).Content);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task NavigationButtonsAndFrameSelectionKeepSharedSample()
    {
        var vm = TestApplicationFactory.CreateMainWindow();
        var window = new MainWindow { DataContext = vm };
        var savedSet = Path.Combine(Path.GetTempPath(), $"QRLab.Tests-{Guid.NewGuid():N}");
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            var button = window.GetVisualDescendants().OfType<Button>().Single(b => b.Command == vm.NavigateCommand && Equals(b.CommandParameter, "02"));
            Assert.NotNull(button.Command); button.Command.Execute(button.CommandParameter);
            Dispatcher.UIThread.RunJobs();
            Assert.Same(vm.Mutations, vm.SelectedScreen);
            Assert.NotNull(vm.Workspace.SelectedSample);
            vm.Mutations.AddCommand.Execute(MutationKind.Perspective);
            vm.Mutations.AngleX = 19;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(19, Assert.IsType<PerspectiveMutationOptions>(vm.Mutations.Operations.Last().Options).AngleX);
            Assert.Same(vm.Workspace.SelectedSample!.OriginalImage, vm.Mutations.PreviewImage);
            vm.Mutations.AddCommand.Execute(MutationKind.GaussianBlur);
            var blurred = vm.Mutations.PreviewImage;
            vm.Mutations.Blur = 3;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(3, Assert.IsType<GaussianBlurOptions>(vm.Mutations.SelectedOperation!.Options).Sigma);
            Assert.NotSame(blurred, vm.Mutations.PreviewImage);
            Assert.NotNull(vm.Mutations.SelectedOperation);
            vm.Mutations.SelectedFrame = vm.Mutations.Frames[0];
            Assert.Null(vm.Mutations.SelectedOperation);
            Assert.Same(vm.Workspace.SelectedSample.OriginalImage, vm.Mutations.PreviewImage);
            vm.Workspace.NextCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.NotNull(vm.Workspace.SelectedSample);
            vm.NavigateCommand.Execute("03");
            vm.Reading.RunCommand.Execute(null);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(120, vm.Reading.ResultsChecked);
            var selected = vm.Workspace.SelectedSample;
            vm.Reading.Filter = "Ошибки"; vm.Reading.Search = "no-match";
            Dispatcher.UIThread.RunJobs();
            Assert.Same(selected, vm.Workspace.SelectedSample);
            vm.NavigateCommand.Execute("01"); Dispatcher.UIThread.RunJobs();
            vm.Generation.OpenLibraryCommand.Execute(null); Dispatcher.UIThread.RunJobs();
            Assert.Contains(window.GetVisualDescendants().OfType<Button>(), b => b.Command == vm.Generation.LoadSetCommand);
            vm.Generation.LoadSetCommand.Execute(vm.Workspace.LibrarySets[0]); Dispatcher.UIThread.RunJobs();
            Assert.Equal(12, vm.Workspace.Samples.Count);
            vm.Generation.ExportCommand.Execute(null); Dispatcher.UIThread.RunJobs();
            Assert.Contains(window.GetVisualDescendants().OfType<Button>(), b => b.Command == vm.Generation.ConfirmExportCommand);
            vm.Generation.ExportDestination = savedSet;
            await vm.Generation.ConfirmExportCommand.ExecuteAsync(null); Dispatcher.UIThread.RunJobs();
            Assert.False(vm.Generation.IsExportOpen);
            Assert.Contains("Сохранён набор", vm.Workspace.Notice);
            Assert.True(File.Exists(Path.Combine(savedSet, "qrlab-set.json")));
        }
        finally
        {
            window.Close();
            if (Directory.Exists(savedSet)) Directory.Delete(savedSet, recursive: true);
        }
    }
}
