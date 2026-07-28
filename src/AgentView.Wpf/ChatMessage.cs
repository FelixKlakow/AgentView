using System.Collections.ObjectModel;

namespace AgentView.Wpf;

/// <summary>One entry of an agent conversation; tool messages carry their calls in <see cref="ToolCalls"/>.</summary>
public class ChatMessage : ObservableObject
{
    private string _id = "";
    private MessageRole _role;
    private string _content = "";
    private DateTimeOffset _timestamp = DateTimeOffset.Now;
    private string? _roleLabel;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public MessageRole Role
    {
        get => _role;
        set => SetProperty(ref _role, value);
    }

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public DateTimeOffset Timestamp
    {
        get => _timestamp;
        set => SetProperty(ref _timestamp, value);
    }

    /// <summary>Optional label shown instead of the default role name.</summary>
    public string? RoleLabel
    {
        get => _roleLabel;
        set => SetProperty(ref _roleLabel, value);
    }

    public ObservableCollection<ToolCall> ToolCalls { get; } = [];
}
