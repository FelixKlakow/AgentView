using System.Windows;
using System.Windows.Controls;

namespace AgentView.Wpf;

/// <summary>Card for a single <see cref="AgentView.Wpf.ToolCall"/> with a state badge and collapsible input/output sections.</summary>
public class ToolCallCard : Control
{
    public static readonly DependencyProperty ToolCallProperty = DependencyProperty.Register(
        nameof(ToolCall), typeof(ToolCall), typeof(ToolCallCard), new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayModeProperty = DependencyProperty.Register(
        nameof(DisplayMode), typeof(ToolCallDisplayMode), typeof(ToolCallCard),
        new PropertyMetadata(ToolCallDisplayMode.Collapsible));

    static ToolCallCard()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolCallCard),
            new FrameworkPropertyMetadata(typeof(ToolCallCard)));

    public ToolCall? ToolCall
    {
        get => (ToolCall?)GetValue(ToolCallProperty);
        set => SetValue(ToolCallProperty, value);
    }

    public ToolCallDisplayMode DisplayMode
    {
        get => (ToolCallDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }
}
