using System.Collections.ObjectModel;
using System.Windows;

namespace AgentView.Wpf.Demo;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ChatMessage> _messages = [];
    private int _streamCounter;

    public MainWindow()
    {
        InitializeComponent();
        Chat.Options = new AgentChatOptions
        {
            ShowTimestamps = true,
            EnableMarkdown = true,
            AutoScroll = true,
            ToolCallDisplay = ToolCallDisplayMode.Collapsible,
            Theme = "dark"
        };
        Chat.Messages = _messages;
        Loaded += async (_, _) => await PlayScriptAsync();
    }

    private async Task PlayScriptAsync()
    {
        Add(MessageRole.System, "Session started — connected to workflow run #42.");
        Add(MessageRole.User, "Please check the repo layout and summarize the **build targets**.");

        Add(MessageRole.Assistant,
            """
            Sure — here is what I found:

            ## Build targets

            The repository builds three artifacts:

            - `AgentView.Wpf` — the library
              - packs to NuGet
            - `AgentView.Wpf.Demo` — this sample
            - `AgentView.Wpf.Tests` — NUnit suite

            | Project | Kind | Framework |
            | ------- | ---- | --------- |
            | AgentView.Wpf | Library | net10.0-windows |
            | Demo | WinExe | net10.0-windows |
            | Tests | NUnit | net10.0-windows |

            The core build step is:

            ```powershell
            dotnet build AgentView.Wpf.slnx
            dotnet test AgentView.Wpf.slnx
            ```

            More details in the [README](https://github.com/felixklakow/AgentView.Wpf).
            """);

        // A running tool that completes with output after a delay — proves live mutation works.
        var listCall = new ToolCall
        {
            Id = "tool-1",
            ToolName = "Bash",
            Subtitle = "List repository files",
            Input = "ls -la src/",
            State = ToolState.Running
        };
        AddTool(listCall);

        await Task.Delay(2500);
        listCall.Output = "total 12\ndrwxr-xr-x  4 felix felix 4096 .\ndrwxr-xr-x 10 felix felix 4096 ..\ndrwxr-xr-x  3 felix felix 4096 AgentView.Wpf";
        listCall.State = ToolState.Success;

        await Task.Delay(600);
        var failedCall = new ToolCall
        {
            Id = "tool-2",
            ToolName = "WebFetch",
            Subtitle = "Fetch release notes",
            Input = "{ \"url\": \"https://example.invalid/notes\" }",
            State = ToolState.Running
        };
        AddTool(failedCall);

        await Task.Delay(1500);
        failedCall.Output = "DNS resolution failed for example.invalid";
        failedCall.State = ToolState.Failed;

        Add(MessageRole.Assistant, "The listing succeeded; fetching the release notes failed (`example.invalid` does not resolve). Want me to retry against the real host?");
    }

    private void OnAppendStreaming(object sender, RoutedEventArgs e)
    {
        _streamCounter++;
        Add(MessageRole.Assistant,
            $"Streaming update **#{_streamCounter}** — appended at {DateTimeOffset.Now:HH:mm:ss.fff}. " +
            "With auto-scroll enabled and the view at the bottom, this message keeps the view pinned.");
    }

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        if (Chat.Options is not { } options)
            return;
        options.Theme = options.Theme == "dark" ? "light" : "dark";
    }

    private void Add(MessageRole role, string content)
        => _messages.Add(new ChatMessage
        {
            Id = Guid.NewGuid().ToString("N"),
            Role = role,
            Content = content,
            Timestamp = DateTimeOffset.Now
        });

    private void AddTool(ToolCall call)
    {
        var message = new ChatMessage
        {
            Id = Guid.NewGuid().ToString("N"),
            Role = MessageRole.Tool,
            Timestamp = DateTimeOffset.Now
        };
        message.ToolCalls.Add(call);
        _messages.Add(message);
    }
}
