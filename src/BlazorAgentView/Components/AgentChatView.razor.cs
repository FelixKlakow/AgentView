using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorAgentView.Models;
using System.Text.RegularExpressions;

namespace BlazorAgentView.Components;

public partial class AgentChatView : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public IEnumerable<ChatMessage> Messages { get; set; } = Enumerable.Empty<ChatMessage>();
    [Parameter] public RenderFragment? UserInputContent { get; set; }
    [Parameter] public string? SystemPrompt { get; set; }
    [Parameter] public AgentChatOptions Options { get; set; } = new();
    [Parameter] public EventCallback<string> OnCancelTool { get; set; }
    [Parameter] public RenderFragment<ToolCall>? ToolContentTemplate { get; set; }

    private List<ChatMessage> _messageList = new();
    private ElementReference _messagesRef;
    private IJSObjectReference? _jsModule;

    // Tracks the last `Messages` parameter reference we synced from. When the
    // caller passes a new collection reference we re-seed `_messageList`; when
    // they keep passing the same reference (e.g. a list they mutate themselves,
    // or a SignalR-driven list) we leave imperative-mode state intact.
    private IEnumerable<ChatMessage>? _lastSyncedMessages;
    // Snapshot of messages count after the previous render so we can detect
    // whether new content was added since the last paint.
    private int _lastRenderedCount;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorAgentView/blazor-agent-view.js");
            }
            catch (JSDisconnectedException)
            {
                // Circuit gone (e.g. user navigated away) - safe to ignore.
            }
            catch (InvalidOperationException)
            {
                // JS interop is unavailable (e.g. during static SSR / prerender).
            }
            catch (TaskCanceledException)
            {
                // Component disposed before import completed.
            }
        }

        var currentCount = _messageList.Count;
        var grew = currentCount > _lastRenderedCount;
        _lastRenderedCount = currentCount;

        if (Options.AutoScroll && (firstRender || grew))
        {
            await ScrollToBottomAsync(force: firstRender);
        }
    }

    protected override void OnParametersSet()
    {
        // Only re-seed the internal list when the caller hands us a new
        // collection reference. If they pass the same reference (mutating it
        // in place or driving updates via the imperative API) we preserve any
        // appended content / tool-state changes that imperative callers have
        // already applied.
        if (!ReferenceEquals(Messages, _lastSyncedMessages))
        {
            _messageList = Messages.ToList();
            _lastSyncedMessages = Messages;
            // Reset the rendered-count snapshot so the next render scrolls
            // (the message set has changed wholesale).
            _lastRenderedCount = 0;
        }
    }

    private async Task ScrollToBottomAsync(bool force = false)
    {
        if (_jsModule is null) return;
        try
        {
            // Skip scrolling when the user has scrolled up to read older
            // messages (unless this is the initial render).
            if (!force)
            {
                var nearBottom = await _jsModule.InvokeAsync<bool>("isNearBottom", _messagesRef);
                if (!nearBottom) return;
            }
            await _jsModule.InvokeVoidAsync("scrollToBottom", _messagesRef);
        }
        catch (JSDisconnectedException) { /* circuit gone */ }
        catch (InvalidOperationException) { /* JS unavailable */ }
        catch (TaskCanceledException) { /* component disposed */ }
    }

    public void AddMessage(ChatMessage message)
    {
        _messageList.Add(message);
        StateHasChanged();
    }

    public void AppendToMessage(string id, string text)
    {
        var message = _messageList.FirstOrDefault(m => m.Id == id);
        if (message != null)
        {
            message.Content += text;
            StateHasChanged();
        }
    }

    public void UpdateToolState(string messageId, string toolCallId, ToolState state, string? output = null)
    {
        var message = _messageList.FirstOrDefault(m => m.Id == messageId);
        if (message != null)
        {
            var toolCall = message.ToolCalls.FirstOrDefault(t => t.Id == toolCallId);
            if (toolCall != null)
            {
                toolCall.State = state;
                if (output != null) toolCall.Output = output;
                StateHasChanged();
            }
        }
    }

    private string ThemeClass => Options.Theme != null ? $"bav-theme-{Options.Theme}" : string.Empty;

    // Validate CSS custom-property names: must start with `--` and only contain
    // ASCII letters, digits, hyphens or underscores. Disallow values containing
    // `;`, newlines or `</` so callers can't smuggle in additional declarations
    // or close the host element's style attribute.
    private static readonly Regex _cssVarNameRegex = new(@"^--[A-Za-z0-9_-]+$", RegexOptions.Compiled);

    private static bool IsSafeCssValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        foreach (var c in value)
        {
            if (c == ';' || c == '\r' || c == '\n' || c == '<' || c == '>' || c == '{' || c == '}' || c == '"' || c == '\'')
                return false;
        }
        return true;
    }

    private string CssVariableStyle
    {
        get
        {
            if (Options.CssVariables.Count == 0) return string.Empty;
            var safe = Options.CssVariables
                .Where(kv => kv.Key is not null
                             && _cssVarNameRegex.IsMatch(kv.Key)
                             && IsSafeCssValue(kv.Value))
                .Select(kv => $"{kv.Key}: {kv.Value}");
            return string.Join("; ", safe);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            try
            {
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException) { /* circuit gone */ }
            catch (InvalidOperationException) { /* JS unavailable */ }
            catch (TaskCanceledException) { /* already cancelled */ }
            finally
            {
                _jsModule = null;
            }
        }
        GC.SuppressFinalize(this);
    }
}
