using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using QRLab.View.ViewModels;


namespace QRLab.View.Views;

public partial class GenerationView : UserControl
{
    public GenerationView() => InitializeComponent();

    private async void BrowseLoadFolder(object? sender, RoutedEventArgs e) => await BrowseFolder(false);
    private async void BrowseSaveFolder(object? sender, RoutedEventArgs e) => await BrowseFolder(true);

    private async Task BrowseFolder(bool save)
    {
        if (DataContext is not GenerationViewModel model || TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage) return;
        try
        {
            var folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = save ? "Выберите пустую папку для сохранения набора" : "Открыть сохранённый набор",
                AllowMultiple = false
            });
            if (folders.Count == 0) return;
            using var folder = folders[0];
            if (folder.TryGetLocalPath() is not { } path)
            {
                model.Workspace.ShowError("Выберите локальную папку.");
                return;
            }
            if (save) model.ExportDestination = path;
            else model.LoadDirectory = path;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
        { model.Workspace.ShowError(exception.Message); }
    }
}
