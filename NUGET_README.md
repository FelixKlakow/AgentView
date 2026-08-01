# BlazorAgentView

A Blazor component library for rendering AI agent chat interfaces with first-class tool call visualisation, Markdown support, streaming, dark mode, and full per-message customisation — all in a single `<AgentChatView>` component.

[![NuGet](https://img.shields.io/nuget/v/BlazorAgentView?style=flat-square)](https://www.nuget.org/packages/BlazorAgentView)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](https://github.com/FelixKlakow/BlazorAgentView/blob/main/LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square)](https://dotnet.microsoft.com/)

Full documentation and source: **[github.com/FelixKlakow/BlazorAgentView](https://github.com/FelixKlakow/BlazorAgentView)**

---

## Features

- 💬 **Chat bubbles** — user bubbles, borderless agent messages, collapsible system-prompt banner
- 🔧 **Tool call cards** — 4 display modes, cancel button, animated state icons, custom type icons
- 🏷️ **Tool subtitles** — optional secondary text beside the tool name
- ✍️ **Markdown** — Markdig pipeline, swappable via `IMarkdownRenderer`
- 📡 **Streaming** — animated typing indicator, incremental token append
- 🌙 **Dark mode** — built-in dark theme + full CSS variable theming
- 🏷️ **Per-message customisation** — custom labels, timestamps, and `RenderFragment` content
- 🖼️ **Custom tool renderers** — `RenderFragment<ToolCall>` for any tool type
- 🔍 **Virtualisation** — opt-in for long chat histories

---

## Quick Start

### 1 — Install

```
dotnet add package BlazorAgentView
```

### 2 — Add the stylesheet

```html
<link rel="stylesheet" href="_content/BlazorAgentView/blazor-agent-view.css" />
```

### 3 — Add the using

```razor
@using BlazorAgentView.Components
@using BlazorAgentView.Models
```

### 4 — Drop in the component

```razor
@code {
    private List<ChatMessage> _messages = new()
    {
        new ChatMessage { Role = MessageRole.User,      Content = "Hello!" },
        new ChatMessage { Role = MessageRole.Assistant, Content = "Hi! How can I help?" }
    };
}

<div style="height: 600px;">
    <AgentChatView Messages="_messages" />
</div>
```

> ⚠️ The component fills **100% of its parent's height**. Always give the parent an explicit height.

---

## Tool call type icons

Each `ToolCall` can carry an optional `RenderFragment? Icon` that is shown on the right side of the card header as a visual type indicator:

```csharp
new ToolCall
{
    ToolName = "read_file",
    Subtitle = "src/Program.cs",
    State    = ToolState.Success,
    Icon     = @<svg viewBox="0 0 16 16" width="14" height="14" fill="currentColor">
                   <path d="M4 1h6l4 4v10H4V1zm6 0v4h4" stroke="currentColor" stroke-width="1.2" fill="none" stroke-linejoin="round" />
               </svg>
}
```

---

## Changelog

### 1.1.0
- **Custom tool type icons** — `ToolCall.Icon` (`RenderFragment?`) renders a custom SVG/HTML icon on the right side of the tool card header as a visual type indicator.
- **`Subtitle` on `ToolCall`** — optional secondary text displayed next to the tool name for at-a-glance summaries (file path, search query, etc.).
- **`ShowSystemPromptBanner` option** — hide the system-prompt banner independently via `AgentChatOptions.ShowSystemPromptBanner = false`.
- **Per-tool `DisplayMode` override** — each `ToolCall` can now override the global `AgentChatOptions.ToolCallDisplay`.

### 1.0.0
- Initial release: `<AgentChatView>` component, tool call cards, Markdown rendering, streaming, dark mode, CSS variable theming, virtualisation.
