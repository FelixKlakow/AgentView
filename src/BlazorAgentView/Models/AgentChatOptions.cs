using BlazorAgentView.Services;

namespace BlazorAgentView.Models;

public class AgentChatOptions
{
    public bool ShowTimestamps { get; set; } = true;
    public bool EnableMarkdown { get; set; } = true;
    public bool EnableVirtualization { get; set; } = false;
    /// <summary>
    /// When <c>true</c> (default), the messages container scrolls to the bottom
    /// when new messages are appended and the user is already near the bottom.
    /// Set to <c>false</c> to opt out of auto-scrolling entirely.
    /// </summary>
    public bool AutoScroll { get; set; } = true;
    public string? Theme { get; set; }
    public Dictionary<string, string> CssVariables { get; set; } = new();

    /// <summary>
    /// Controls whether tool-call cards can be expanded/collapsed by the user
    /// and what their initial state is. Defaults to <see cref="ToolCallDisplayMode.Collapsible"/>.
    /// </summary>
    public ToolCallDisplayMode ToolCallDisplay { get; set; } = ToolCallDisplayMode.Collapsible;

    /// <summary>
    /// When <c>false</c> (default) assistant messages are rendered as plain text without
    /// a coloured bubble. Set to <c>true</c> to restore the bubble background.
    /// User messages always use a bubble.
    /// </summary>
    public bool EnableAssistantBubble { get; set; } = false;

    /// <summary>
    /// When <c>true</c> the system-prompt banner renders its text as Markdown.
    /// Defaults to <c>false</c> (plain pre-formatted text).
    /// </summary>
    public bool SystemPromptMarkdown { get; set; } = false;

    /// <summary>
    /// When <c>false</c> the system-prompt banner is hidden even when a
    /// <c>SystemPrompt</c> value is supplied.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ShowSystemPromptBanner { get; set; } = true;

    /// <summary>
    /// Overrides the <see cref="IMarkdownRenderer"/> used for this chat view.
    /// <c>null</c> (default) resolves the renderer from DI and falls back to
    /// <see cref="DefaultMarkdownRenderer"/> when none is registered.
    /// </summary>
    /// <remarks>
    /// Use this to tune Markdig per view, e.g.
    /// <c>new DefaultMarkdownRenderer(DefaultMarkdownRenderer.CreatePipeline(b =&gt; ...))</c>.
    /// A renderer that throws never breaks the chat view: the affected message is
    /// rendered as plain text instead.
    /// </remarks>
    public IMarkdownRenderer? MarkdownRenderer { get; set; }
}
