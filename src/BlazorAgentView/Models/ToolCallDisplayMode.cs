namespace BlazorAgentView.Models;

/// <summary>
/// Controls how tool-call cards expose their input/output details.
/// </summary>
public enum ToolCallDisplayMode
{
    /// <summary>
    /// User can expand and collapse the tool call. Starts collapsed (default).
    /// </summary>
    Collapsible = 0,

    /// <summary>
    /// User can expand and collapse the tool call. Starts expanded.
    /// </summary>
    CollapsibleExpanded = 1,

    /// <summary>
    /// Details are always visible. The toggle button is hidden.
    /// </summary>
    AlwaysExpanded = 2,

    /// <summary>
    /// Only the header row is shown. Details cannot be opened.
    /// </summary>
    HeaderOnly = 3,
}
