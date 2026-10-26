using CommunityToolkit.Mvvm.ComponentModel;


namespace QRLab.View.ViewModels;

public abstract class ScreenViewModel(string number, string title, string description) : ObservableObject
{
    public string Number { get; } = number;
    public string Title { get; } = title;
    public string Description { get; } = description;
}
