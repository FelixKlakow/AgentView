using Microsoft.AspNetCore.Components;

namespace BlazorAgentView.Services;

/// <summary>
/// Helpers for calling an <see cref="IMarkdownRenderer"/> from inside a render tree.
/// </summary>
public static class MarkdownRendererExtensions
{
    /// <summary>
    /// Renders <paramref name="markdown"/> without ever letting the renderer's
    /// exceptions escape. A component that renders markdown inside its render tree
    /// must not propagate failures: on Blazor Server an exception thrown while
    /// building the render tree tears down the whole circuit, so one malformed
    /// message would freeze the entire application.
    /// </summary>
    /// <returns>
    /// <c>true</c> when <paramref name="html"/> holds the rendered markup;
    /// <c>false</c> when the renderer failed and the caller should fall back to
    /// plain text.
    /// </returns>
    public static bool TryRender(this IMarkdownRenderer renderer, string markdown, out MarkupString html)
    {
        ArgumentNullException.ThrowIfNull(renderer);

        try
        {
            html = renderer.Render(markdown);
            return true;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            html = default;
            return false;
        }
    }
}
