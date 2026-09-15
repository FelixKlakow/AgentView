using System.Text;
using System.Text.Encodings.Web;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorAgentView.Services;

/// <summary>
/// Default <see cref="IMarkdownRenderer"/> implementation backed by Markdig.
/// </summary>
/// <remarks>
/// <para>
/// Raw HTML (inline and block) is disabled in the Markdig pipeline so that any
/// HTML embedded in the source markdown is escaped rather than emitted verbatim.
/// This eliminates the primary XSS surface (e.g. <c>&lt;script&gt;</c>,
/// <c>&lt;iframe&gt;</c>, <c>on*</c> handler attributes, <c>javascript:</c> URLs
/// inside raw HTML, SVG payloads). Hyperlinks generated from markdown link syntax
/// are URI-validated by Markdig and rendered through anchor tags only.
/// Consumers who require richer HTML support should supply their own
/// <see cref="IMarkdownRenderer"/> implementation that integrates a hardened
/// allow-list HTML sanitizer.
/// </para>
/// <para>
/// <see cref="Render"/> never throws. Markdig rejects some inputs outright — most
/// notably text that trips its nesting limit, which agent output hits easily
/// because a block of 32 or more pipe-delimited lines without a table separator
/// row produces one nested delimiter inline per <c>|</c>. Since rendering happens
/// inside a component's render tree, an escaping exception would take down a
/// Blazor Server circuit, so a failed render degrades instead: first a retry with
/// a plain CommonMark pipeline (no pipe tables, no advanced inlines), then
/// HTML-encoded plain text with the line breaks preserved.
/// </para>
/// </remarks>
public class DefaultMarkdownRenderer : IMarkdownRenderer
{
    private static readonly MarkdownPipeline _defaultPipeline = CreatePipeline();

    /// <summary>
    /// Extension-free safety net: without the pipe-table extension the inputs that
    /// trip Markdig's nesting limit parse fine, so most markdown still survives.
    /// </summary>
    private static readonly MarkdownPipeline _fallbackPipeline =
        new MarkdownPipelineBuilder().DisableHtml().Build();

    private readonly MarkdownPipeline _pipeline;
    private readonly ILogger<DefaultMarkdownRenderer>? _logger;

    /// <summary>
    /// Creates a renderer using the default pipeline
    /// (<c>UseAdvancedExtensions()</c> with raw HTML disabled).
    /// </summary>
    public DefaultMarkdownRenderer() : this(null, null)
    {
    }

    /// <summary>
    /// Creates a renderer using a custom Markdig pipeline.
    /// </summary>
    /// <param name="pipeline">
    /// The pipeline to render with, or <c>null</c> for the default one.
    /// Use <see cref="CreatePipeline"/> to build a tuned pipeline that keeps
    /// raw HTML disabled.
    /// </param>
    /// <param name="logger">
    /// Optional logger; failed renders are reported at <c>Debug</c> level so hosts
    /// can see how often the fallback fires.
    /// </param>
    public DefaultMarkdownRenderer(MarkdownPipeline? pipeline, ILogger<DefaultMarkdownRenderer>? logger = null)
    {
        _pipeline = pipeline ?? _defaultPipeline;
        _logger = logger;
    }

    /// <summary>
    /// Builds a Markdig pipeline with the library defaults
    /// (<c>UseAdvancedExtensions()</c>), optionally adjusted by
    /// <paramref name="configure"/>. Raw HTML is disabled after
    /// <paramref name="configure"/> runs, so the XSS guarantee documented on this
    /// class holds for tuned pipelines too.
    /// </summary>
    public static MarkdownPipeline CreatePipeline(Action<MarkdownPipelineBuilder>? configure = null)
    {
        var builder = new MarkdownPipelineBuilder().UseAdvancedExtensions();
        configure?.Invoke(builder);
        builder.DisableHtml();
        return builder.Build();
    }

    /// <inheritdoc />
    public MarkupString Render(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return new MarkupString(string.Empty);

        try
        {
            return new MarkupString(Markdown.ToHtml(markdown, _pipeline));
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            _logger?.LogDebug(ex, "Markdown rendering failed; retrying without extensions.");
        }

        try
        {
            return new MarkupString(Markdown.ToHtml(markdown, _fallbackPipeline));
        }
        catch (Exception ex) when (IsRecoverable(ex))
        {
            _logger?.LogDebug(ex, "Markdown rendering failed; falling back to plain text.");
        }

        return RenderPlainText(markdown);
    }

    /// <summary>
    /// HTML-encodes <paramref name="text"/> and keeps its line breaks. This is the
    /// last-resort representation used when no pipeline can render the input;
    /// custom <see cref="IMarkdownRenderer"/> implementations can reuse it.
    /// </summary>
    public static MarkupString RenderPlainText(string text)
    {
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        var sb = new StringBuilder(text.Length + 16);
        sb.Append("<p>");
        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                sb.Append("<br />");
            sb.Append(HtmlEncoder.Default.Encode(lines[i]));
        }
        sb.Append("</p>");

        return new MarkupString(sb.ToString());
    }

    private static bool IsRecoverable(Exception ex)
        => ex is not OutOfMemoryException and not OperationCanceledException;
}
