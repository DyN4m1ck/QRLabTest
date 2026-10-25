using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using QRLab.View.ViewModels;

namespace QRLab.View.Views;

public partial class ReadingView : UserControl
{
    public ReadingView() => InitializeComponent();

    private async void BrowseCsvFile(object? sender, RoutedEventArgs args)
    {
        if (DataContext is not ReadingViewModel model || TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage) return;
        try
        {
            using var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить результаты декодирования", SuggestedFileName = "results.csv",
                DefaultExtension = "csv", ShowOverwritePrompt = true,
                FileTypeChoices = [new FilePickerFileType("CSV") { Patterns = ["*.csv"] }]
            });
            if (file is null) return;
            if (file.TryGetLocalPath() is { } path) model.ExportDestination = path;
            else model.Workspace.ShowError("Выберите локальный CSV-файл.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
        { model.Workspace.ShowError(exception.Message); }
    }
}