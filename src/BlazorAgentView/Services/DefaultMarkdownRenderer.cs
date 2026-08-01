using Markdig;
using Microsoft.AspNetCore.Components;

namespace BlazorAgentView.Services;

/// <summary>
/// Default <see cref="IMarkdownRenderer"/> implementation backed by Markdig.
/// </summary>
/// <remarks>
/// Raw HTML (inline and block) is disabled in the Markdig pipeline so that any
/// HTML embedded in the source markdown is escaped rather than emitted verbatim.
/// This eliminates the primary XSS surface (e.g. <c>&lt;script&gt;</c>,
/// <c>&lt;iframe&gt;</c>, <c>on*</c> handler attributes, <c>javascript:</c> URLs
/// inside raw HTML, SVG payloads). Hyperlinks generated from markdown link syntax
/// are URI-validated by Markdig and rendered through anchor tags only.
/// Consumers who require richer HTML support should supply their own
/// <see cref="IMarkdownRenderer"/> implementation that integrates a hardened
/// allow-list HTML sanitizer.
/// </remarks>
public class DefaultMarkdownRenderer : IMarkdownRenderer
{
    private static readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public MarkupString Render(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return new MarkupString(string.Empty);

        return new MarkupString(Markdown.ToHtml(markdown, _pipeline));
    }
}
