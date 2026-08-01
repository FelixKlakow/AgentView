# BlazorAgentView.SignalR

SignalR integration package for [BlazorAgentView](https://www.nuget.org/packages/BlazorAgentView) - provides shared hub constants and helpers for streaming AI agent messages over a real-time SignalR connection.

[![NuGet](https://img.shields.io/nuget/v/BlazorAgentView.SignalR?style=flat-square)](https://www.nuget.org/packages/BlazorAgentView.SignalR)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](../../LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square)](https://dotnet.microsoft.com/)

---

## When do I need this?

Use this package when you want to drive an `<AgentChatView>` component with a **real-time SignalR connection** - for example, streaming AI responses from a server-side LLM pipeline to a Blazor client without holding a long HTTP request open.

If you are using **Blazor Server** with `Interactive Server` render mode and calling your AI backend directly in C# (no Hub needed), you can use the base `BlazorAgentView` package alone and skip this one.

---

## Installation

Install both packages:

```
dotnet add package BlazorAgentView
dotnet add package BlazorAgentView.SignalR
```

---

## How it works

The package exposes `AgentChatSignalRHub` - a static class with the canonical method name constants used by the hub. Sharing these between your server hub and your Blazor client avoids hardcoded strings on both sides.

```csharp
public static class AgentChatSignalRHub
{
    public const string ReceiveMessage = "ReceiveMessage";  // full ChatMessage
    public const string AppendContent  = "AppendContent";   // streaming token
    public const string UpdateTool     = "UpdateTool";      // tool state change
}
```

---

## Setup

### 1 - Server: define the Hub

```csharp
using BlazorAgentView.SignalR;
using Microsoft.AspNetCore.SignalR;

public class AgentHub : Hub
{
    public async Task StartAgentRun(string userMessage)
    {
        // Echo the user message back
        await Clients.Caller.SendAsync(AgentChatSignalRHub.ReceiveMessage, new
        {
            role      = "user",
            content   = userMessage,
            timestamp = DateTimeOffset.UtcNow
        });

        // Start a streaming assistant reply
        var msgId = Guid.NewGuid().ToString();
        await Clients.Caller.SendAsync(AgentChatSignalRHub.ReceiveMessage, new
        {
            id          = msgId,
            role        = "assistant",
            content     = "",
            isStreaming = true,
            timestamp   = DateTimeOffset.UtcNow
        });

        // Stream tokens one by one
        await foreach (var token in MyAiService.StreamAsync(userMessage))
        {
            await Clients.Caller.SendAsync(AgentChatSignalRHub.AppendContent, msgId, token);
        }
    }
}
```

### 2 - Server: register the Hub

```csharp
// Program.cs
app.MapHub<AgentHub>("/agent-hub");
```

### 3 - Client: connect and wire up the component

```razor
@page "/chat"
@rendermode InteractiveServer
@using BlazorAgentView.Components
@using BlazorAgentView.Models
@using BlazorAgentView.SignalR
@using Microsoft.AspNetCore.SignalR.Client
@inject NavigationManager Navigation
@implements IAsyncDisposable

<div style="height: 600px;">
    <AgentChatView @ref="_chatView" Messages="_messages" />
</div>

@code {
    private AgentChatView _chatView = default!;
    private List<ChatMessage> _messages = new();
    private HubConnection? _hub;

    protected override async Task OnInitializedAsync()
    {
        _hub = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/agent-hub"))
            .WithAutomaticReconnect()
            .Build();

        // Full message (user echo or initial assistant message)
        _hub.On<ChatMessageDto>(AgentChatSignalRHub.ReceiveMessage, dto =>
        {
            var msg = new ChatMessage
            {
                Id          = dto.Id ?? Guid.NewGuid().ToString(),
                Role        = Enum.Parse<MessageRole>(dto.Role, ignoreCase: true),
                Content     = dto.Content,
                IsStreaming = dto.IsStreaming,
                Timestamp   = dto.Timestamp
            };
            InvokeAsync(() => _chatView.AddMessage(msg));
        });

        // Streaming token - appended to an existing message by id
        _hub.On<string, string>(AgentChatSignalRHub.AppendContent, (id, token) =>
        {
            InvokeAsync(() => _chatView.AppendToMessage(id, token));
        });

        // Tool state update
        _hub.On<string, string, string>(AgentChatSignalRHub.UpdateTool, (msgId, toolId, state) =>
        {
            var toolState = Enum.Parse<ToolState>(state, ignoreCase: true);
            InvokeAsync(() => _chatView.UpdateToolState(msgId, toolId, toolState));
        });

        await _hub.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
            await _hub.DisposeAsync();
    }
}
```

---

## Hub method constants

| Constant | Value | Direction | Payload |
|---|---|---|---|
| `ReceiveMessage` | `"ReceiveMessage"` | Server → Client | Full message DTO |
| `AppendContent` | `"AppendContent"` | Server → Client | `(string id, string token)` |
| `UpdateTool` | `"UpdateTool"` | Server → Client | `(string msgId, string toolId, string state)` |

---

## Related

- [BlazorAgentView](https://www.nuget.org/packages/BlazorAgentView) - the core UI component
- [Microsoft.AspNetCore.SignalR.Client](https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client) - SignalR client (transitive dependency of this package)

---

## Changelog

### 1.0.0
- Initial release: `AgentChatSignalRHub` constants (`ReceiveMessage`, `AppendContent`, `UpdateTool`) for wiring a SignalR hub to `<AgentChatView>`.

---

## License

MIT © Felix Klakow
