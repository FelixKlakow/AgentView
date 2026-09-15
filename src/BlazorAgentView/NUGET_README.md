# BlazorAgentView

A Blazor component library for rendering AI agent chat interfaces with first-class tool call visualisation, Markdown support, streaming, dark mode, and full per-message customisation — all in a single `<AgentChatView>` component.

[![NuGet](https://img.shields.io/nuget/v/BlazorAgentView?style=flat-square)](https://www.nuget.org/packages/BlazorAgentView)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](https://github.com/FelixKlakow/AgentView/blob/main/LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square)](https://dotnet.microsoft.com/)

Full documentation and source: **[github.com/FelixKlakow/AgentView](https://github.com/FelixKlakow/AgentView)**

---

## Features

- 💬 **Chat bubbles** — user bubbles, borderless agent messages, collapsible system-prompt banner
- 🔧 **Tool call cards** — 4 display modes, cancel button, animated state icons, custom type icons
- 🏷️ **Tool subtitles** — optional secondary text beside the tool name
- ✍️ **Markdown** — Markdig pipeline, swappable via `IMarkdownRenderer`, never throws out of the render tree
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

## Markdown rendering

Markdown is rendered with Markdig (`UseAdvancedExtensions()`, raw HTML disabled) through the
`IMarkdownRenderer` service. Register the default one — and optionally tune the pipeline — with:

```csharp
builder.Services.AddBlazorAgentView();

// or, with a tuned pipeline (UseAdvancedExtensions() is already applied,
// raw HTML stays disabled whatever the callback does):
builder.Services.AddBlazorAgentView(pipeline => pipeline.UseEmojiAndSmiley());
```

A renderer can also be set per view via `AgentChatOptions.MarkdownRenderer`, which takes
precedence over the DI registration.

### Failed renders degrade, they never crash

Markdig rejects some inputs outright — most notably text that trips its nesting limit, which
agent output hits easily: a block of 32 or more pipe-delimited lines *without* a table separator
row produces one nested delimiter inline per `|`. Rendering happens inside the component's render
tree, so an escaping exception would terminate a Blazor Server circuit and freeze the whole
application.

Instead, rendering degrades in steps: the configured pipeline, then a plain CommonMark pipeline
(no pipe tables), then HTML-encoded plain text with the line breaks preserved. The same guard
wraps the call site, so a custom `IMarkdownRenderer` that throws only costs that one message its
formatting; the message is shown as plain text and the failure is cached so streaming deltas do
not re-parse it.

> 💡 This covers markdown only. On Blazor Server, wrapping `<AgentChatView>` in an
> `<ErrorBoundary>` is still recommended — any component can throw.

---

## Changelog

### Unreleased
- **Markdown rendering never takes down the circuit** — `DefaultMarkdownRenderer.Render` no longer
  throws (it degrades to a CommonMark pipeline and then to encoded plain text), and `MessageBubble`
  / `SystemPromptBanner` fall back to plain text when a custom `IMarkdownRenderer` throws. Fixes
  the freeze caused by 32+ pipe-delimited lines without a table separator row.
- **Configurable Markdig pipeline** — `AddBlazorAgentView(pipeline => ...)` and
  `DefaultMarkdownRenderer.CreatePipeline(...)`.
- **`AgentChatOptions.MarkdownRenderer`** — per-view renderer override.

### 1.1.0
- **Custom tool type icons** — `ToolCall.Icon` (`RenderFragment?`) renders a custom SVG/HTML icon on the right side of the tool card header as a visual type indicator.
- **`Subtitle` on `ToolCall`** — optional secondary text displayed next to the tool name for at-a-glance summaries (file path, search query, etc.).
- **`ShowSystemPromptBanner` option** — hide the system-prompt banner independently via `AgentChatOptions.ShowSystemPromptBanner = false`.
- **Per-tool `DisplayMode` override** — each `ToolCall` can now override the global `AgentChatOptions.ToolCallDisplay`.

### 1.0.0
- Initial release: `<AgentChatView>` component, tool call cards, Markdown rendering, streaming, dark mode, CSS variable theming, virtualisation.
