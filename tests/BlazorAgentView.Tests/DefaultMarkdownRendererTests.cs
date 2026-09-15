using BlazorAgentView.Services;
using Markdig;

namespace BlazorAgentView.Tests;

[TestFixture]
public class DefaultMarkdownRendererTests
{
    private readonly DefaultMarkdownRenderer _renderer = new();

    [Test]
    public void Render_Empty_ReturnsEmpty()
    {
        Assert.That(_renderer.Render(string.Empty).Value, Is.Empty);
    }

    [Test]
    public void Render_SimpleMarkdown_ReturnsHtml()
    {
        var html = _renderer.Render("Hello **world**").Value;

        Assert.That(html, Does.Contain("<strong>world</strong>"));
    }

    // 31 pipe lines stay below Markdig's nesting limit and render through the
    // default (advanced-extensions) pipeline.
    [Test]
    public void Render_PipeLinesBelowNestingLimit_RendersText()
    {
        var html = _renderer.Render(MarkdownInput.PipeLinesWithoutSeparator(31)).Value;

        Assert.That(html, Does.Contain("Story 31"));
    }

    // The regression from the issue: 40 pipe lines without a separator row made
    // Markdig throw, which took down the Blazor Server circuit.
    [Test]
    public void Render_PipeLinesAboveNestingLimit_DoesNotThrowAndKeepsText()
    {
        string html = null!;

        Assert.DoesNotThrow(() => html = _renderer.Render(MarkdownInput.PipeLinesWithoutSeparator(40)).Value);

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("Story 1"));
            Assert.That(html, Does.Contain("Story 40"));
            // The fallback must not emit a half-built table.
            Assert.That(html, Does.Not.Contain("<table"));
        });
    }

    [Test]
    public void Render_PipeLinesAboveNestingLimit_EncodesHtml()
    {
        var html = _renderer.Render(MarkdownInput.PipeLinesWithoutSeparator(40) + "\n<script>alert(1)</script>").Value;

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Not.Contain("<script>"));
            Assert.That(html, Does.Contain("alert(1)"));
        });
    }

    [Test]
    public void Render_LargeValidTable_StillRendersTable()
    {
        var html = _renderer.Render(MarkdownInput.ValidTable(200)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("<table"));
            Assert.That(html, Does.Contain("Story 200"));
        });
    }

    [Test]
    public void RenderPlainText_PreservesLineBreaksAndEncodes()
    {
        var html = DefaultMarkdownRenderer.RenderPlainText("a & b\nc <d>").Value;

        Assert.That(html, Is.EqualTo("<p>a &amp; b<br />c &lt;d&gt;</p>"));
    }

    [Test]
    public void CreatePipeline_WithConfiguration_IsUsedByRenderer()
    {
        // Emojis are not part of UseAdvancedExtensions(), so seeing one replaced
        // proves the callback reached the pipeline the renderer uses.
        var renderer = new DefaultMarkdownRenderer(
            DefaultMarkdownRenderer.CreatePipeline(builder => builder.UseEmojiAndSmiley()));

        var html = renderer.Render(":smiley:").Value;

        Assert.That(html, Does.Not.Contain(":smiley:"));
    }

    [Test]
    public void CreatePipeline_WithConfiguration_KeepsRawHtmlDisabled()
    {
        var renderer = new DefaultMarkdownRenderer(
            DefaultMarkdownRenderer.CreatePipeline(builder => builder.UseEmojiAndSmiley()));

        var html = renderer.Render("<script>alert(1)</script>").Value;

        Assert.That(html, Does.Not.Contain("<script>"));
    }
}
