using Microsoft.AspNetCore.Components;

namespace BlazorAgentView.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<ToolCall> ToolCalls { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public bool IsStreaming { get; set; }
    public RenderFragment? CustomContent { get; set; }

    /// <summary>
    /// Overrides the role label shown in the message header.
    /// Set to <c>null</c> (default) to use the built-in label ("Agent", "You", …).
    /// Set to an empty string <c>""</c> to hide the label entirely.
    /// </summary>
    public string? RoleLabel { get; set; }

    /// <summary>
    /// Overrides <see cref="AgentChatOptions.ShowTimestamps"/> for this individual message.
    /// <c>null</c> (default) inherits the global setting.
    /// </summary>
    public bool? ShowTimestamp { get; set; }
}
