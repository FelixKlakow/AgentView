namespace AgentView.Wpf;

/// <summary>Lifecycle state of a tool invocation.</summary>
public enum ToolState
{
    Pending,
    Running,
    Success,
    Failed,
    Cancelled
}
