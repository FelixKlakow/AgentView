namespace AgentView.Wpf;

/// <summary>Display options for <see cref="AgentChatView"/>; assign a new instance (or re-assign) to apply changes.</summary>
public class AgentChatOptions : ObservableObject
{
    private bool _showTimestamps;
    private bool _enableMarkdown = true;
    private bool _autoScroll = true;
    private ToolCallDisplayMode _toolCallDisplay = ToolCallDisplayMode.Collapsible;
    private string _theme = "dark";

    public bool ShowTimestamps
    {
        get => _showTimestamps;
        set => SetProperty(ref _showTimestamps, value);
    }

    public bool EnableMarkdown
    {
        get => _enableMarkdown;
        set => SetProperty(ref _enableMarkdown, value);
    }

    public bool AutoScroll
    {
        get => _autoScroll;
        set => SetProperty(ref _autoScroll, value);
    }

    public ToolCallDisplayMode ToolCallDisplay
    {
        get => _toolCallDisplay;
        set => SetProperty(ref _toolCallDisplay, value);
    }

    /// <summary>"dark" (default) or "light"; brushes can additionally be overridden via the AgentView.* resource keys.</summary>
    public string Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value ?? "dark");
    }
}
