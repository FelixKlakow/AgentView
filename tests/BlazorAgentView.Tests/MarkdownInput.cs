namespace BlazorAgentView.Tests;

/// <summary>
/// Markdown inputs shared by the renderer and component tests.
/// </summary>
internal static class MarkdownInput
{
    /// <summary>
    /// A block of pipe-delimited lines without a table separator row — the shape of
    /// tool output that LLMs paste verbatim. Markdig's pipe-table extension keeps one
    /// nested delimiter inline per <c>|</c>, so from 32 such lines on the HTML
    /// renderer trips its nesting limit and throws.
    /// </summary>
    public static string PipeLinesWithoutSeparator(int lineCount)
        => string.Join(
            "\n",
            Enumerable.Range(1, lineCount).Select(i => $"#{i} | AreaPath=TPA\\Line\\OIB | State=Done | Title=Story {i}"));

    /// <summary>
    /// A well-formed pipe table with <paramref name="rowCount"/> body rows.
    /// </summary>
    public static string ValidTable(int rowCount)
    {
        var lines = new List<string>(rowCount + 2)
        {
            "| Id | State | Title |",
            "| --- | --- | --- |"
        };

        for (var i = 1; i <= rowCount; i++)
            lines.Add($"| {i} | Done | Story {i} |");

        return string.Join("\n", lines);
    }
}
