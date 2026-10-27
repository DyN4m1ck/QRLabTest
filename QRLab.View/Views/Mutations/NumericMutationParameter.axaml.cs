using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace QRLab.View.Views.Mutations;

public sealed partial class NumericMutationParameter : UserControl
{
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<NumericMutationParameter, string>(nameof(Label), "");
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<NumericMutationParameter, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<NumericMutationParameter, double>(nameof(Minimum));
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<NumericMutationParameter, double>(nameof(Maximum));

    public string Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

    public NumericMutationParameter() => InitializeComponent();
}
