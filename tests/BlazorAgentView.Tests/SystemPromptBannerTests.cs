using BlazorAgentView.Components;
using BlazorAgentView.Models;
using BlazorAgentView.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorAgentView.Tests;

[TestFixture]
public class SystemPromptBannerTests
{
    private sealed class ThrowingMarkdownRenderer : IMarkdownRenderer
    {
        public MarkupString Render(string markdown) => throw new InvalidOperationException("renderer blew up");
    }

    [Test]
    public void Render_ThrowingRenderer_FallsBackToPreformattedText()
    {
        using var ctx = new BunitContext();
        var options = new AgentChatOptions
        {
            SystemPromptMarkdown = true,
            MarkdownRenderer = new ThrowingMarkdownRenderer()
        };

        var cut = ctx.Render<SystemPromptBanner>(parameters => parameters
            .Add(p => p.SystemPrompt, "You are a **helpful** agent.")
            .Add(p => p.Options, options));

        cut.Find("button.bav-system-prompt-toggle").Click(new MouseEventArgs());

        Assert.Multiple(() =>
        {
            Assert.That(cut.Markup, Does.Contain("<pre>"));
            Assert.That(cut.Markup, Does.Contain("You are a **helpful** agent."));
        });
    }
}
