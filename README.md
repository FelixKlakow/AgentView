# AgentView

Chat views for AI agent conversations — user / assistant / system / tool messages, collapsible
tool-call cards with live-updating state, markdown rendering, streaming, and dark/light themes.
One repository, one model, multiple UI stacks.

| Package | Stack | NuGet |
| --- | --- | --- |
| [BlazorAgentView](src/BlazorAgentView) | Blazor | [![NuGet](https://img.shields.io/nuget/v/BlazorAgentView.svg)](https://www.nuget.org/packages/BlazorAgentView) |
| [BlazorAgentView.SignalR](src/BlazorAgentView.SignalR) | Blazor + SignalR | [![NuGet](https://img.shields.io/nuget/v/BlazorAgentView.SignalR.svg)](https://www.nuget.org/packages/BlazorAgentView.SignalR) |
| [AgentView.Wpf](src/AgentView.Wpf) | WPF (Windows) | [![NuGet](https://img.shields.io/nuget/v/AgentView.Wpf.svg)](https://www.nuget.org/packages/AgentView.Wpf) |

## Blazor

```razor
<AgentChatView Messages="@messages" Options="@options" OnSendMessage="HandleSend" />
```

See [src/BlazorAgentView/NUGET_README.md](src/BlazorAgentView/NUGET_README.md) for the full
feature tour (tool-call visualisation, custom per-message renderers, streaming, dark mode) and
[src/BlazorAgentView.SignalR](src/BlazorAgentView.SignalR) for streaming messages over SignalR.

![BlazorAgentView dark theme](assets/screenshot-dark.png)

## WPF

```xml
<Window xmlns:av="clr-namespace:AgentView.Wpf;assembly=AgentView.Wpf">
    <av:AgentChatView x:Name="Chat" />
</Window>
```

See [src/AgentView.Wpf/NUGET_README.md](src/AgentView.Wpf/NUGET_README.md) for usage, theming,
and the pluggable fence renderer (e.g. mermaid diagrams).

## Repository layout

- `src/` — the three library projects, each producing one NuGet package
- `samples/` — runnable demos: `BlazorAgentView.Demo`, `AgentView.Wpf.Demo`
- `tests/` — unit tests

## Building

```bash
dotnet build AgentView.slnx
dotnet test AgentView.slnx
```

Requires the .NET 10 SDK; the WPF projects build on Windows only.

## Releasing

Push a tag `v*` (e.g. `v2026.08.01`) — CI packs all three packages and pushes any new versions
to NuGet (existing versions are skipped). Package versions live in each project's `.csproj`.

## License

[MIT](LICENSE)
