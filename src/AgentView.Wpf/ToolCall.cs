namespace AgentView.Wpf;

/// <summary>A single tool invocation card; mutate <see cref="Output"/> and <see cref="State"/> when the result arrives and the UI updates live.</summary>
public class ToolCall : ObservableObject
{
    private string _id = "";
    private string _toolName = "";
    private string _subtitle = "";
    private string _input = "";
    private string _output = "";
    private ToolState _state = ToolState.Pending;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string ToolName
    {
        get => _toolName;
        set => SetProperty(ref _toolName, value);
    }

    public string Subtitle
    {
        get => _subtitle;
        set => SetProperty(ref _subtitle, value);
    }

    public string Input
    {
        get => _input;
        set => SetProperty(ref _input, value);
    }

    public string Output
    {
        get => _output;
        set => SetProperty(ref _output, value);
    }

    public ToolState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }
}
