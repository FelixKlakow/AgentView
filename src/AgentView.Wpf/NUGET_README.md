# AgentView.Wpf

Native WPF agent-conversation chat view — a WPF sibling of the Blazor package **BlazorAgentView**.
Renders user / assistant / system / tool messages with collapsible tool-call cards, live-updating
tool state, markdown (via [Markdig](https://github.com/xoofx/markdig)), auto-scroll, and a dark
theme by default (light theme included).

![Screenshot placeholder](docs/screenshot.png)

## Usage

```xml
<Window xmlns:av="clr-namespace:AgentView.Wpf;assembly=AgentView.Wpf">
    <av:AgentChatView x:Name="Chat" />
</Window>
```

```csharp
var messages = new ObservableCollection<ChatMessage>();
Chat.Options = new AgentChatOptions
{
    ShowTimestamps = true,
    EnableMarkdown = true,
    AutoScroll = true,
    ToolCallDisplay = ToolCallDisplayMode.Collapsible,
    Theme = "dark"
};
Chat.Messages = messages;

messages.Add(new ChatMessage { Role = MessageRole.User, Content = "Hi!", Timestamp = DateTimeOffset.Now });

// Tool correlation: append the card while the tool runs, then mutate it when the result arrives —
// the UI updates live.
var call = new ToolCall { ToolName = "Bash", Input = "ls -la", State = ToolState.Running };
var toolMessage = new ChatMessage { Role = MessageRole.Tool };
toolMessage.ToolCalls.Add(call);
messages.Add(toolMessage);
// later:
call.Output = "…";
call.State = ToolState.Success;
```

## Theming

`AgentChatOptions.Theme` is `"dark"` (default) or `"light"`. All colors resolve via
`DynamicResource` against the keys in `AgentViewResourceKeys` — override any of them in your
application, window, or directly on the `AgentChatView` to re-theme:

| Key (`AgentViewResourceKeys`) | Blazor CSS variable equivalent | Dark default |
| --- | --- | --- |
| `BackgroundBrushKey` | `--bav-bg` | `#202020` |
| `SurfaceBrushKey` | `--bav-surface` | `#2B2B2B` |
| `SurfaceStrongBrushKey` | `--bav-surface-strong` | `#333333` |
| `BorderBrushKey` | `--bav-border` | `#3A3A3A` |
| `AccentBrushKey` | `--bav-accent` | `#4F8EF7` |
| `TextBrushKey` | — | `#E8E8E8` |
| `SubduedTextBrushKey` | — | `#9A9A9A` |
| `SuccessBrushKey` | — | `#3E9B4F` |
| `ErrorBrushKey` | — | `#D9534F` |
| `MonospaceFontFamilyKey` | — | Cascadia Mono / Consolas |

```xml
<Application.Resources>
    <SolidColorBrush x:Key="{x:Static av:AgentViewResourceKeys.AccentBrushKey}" Color="#FF8800" />
</Application.Resources>
```

The Blazor version's `CssVariables` dictionary does **not** carry over — use the resource keys
above instead. The light palette ships as `Themes/Light.xaml` and can also be merged manually.

## Notes

- `Messages` accepts any `IEnumerable<ChatMessage>`; use an `ObservableCollection` for streaming.
- Message content is selectable (read-only text boxes / rich text).
- Markdown: headings, emphasis, inline code, fenced code blocks (no syntax highlighting yet),
  nested lists, links, blockquotes, pipe tables, horizontal rules.
- Markdown rendering never throws: content that Markdig rejects — a long block of pipe-delimited
  lines keeps one nested inline per `|` — falls back to plain text for that message instead of
  reaching the dispatcher as an unhandled exception. Nesting past 64 levels is flattened to text
  rather than recursed over, and a `FenceRenderer` that throws falls back to the default code
  block.
- Build: `dotnet build AgentView.slnx` · test: `dotnet test AgentView.slnx` ·
  demo: `dotnet run --project samples/AgentView.Wpf.Demo`.

## License

MIT © 2026 Felix Klakow
