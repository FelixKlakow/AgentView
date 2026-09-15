using BlazorAgentView.Components;
using BlazorAgentView.Models;
using BlazorAgentView.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAgentView.Tests;

[TestFixture]
public class MessageBubbleTests
{
    private sealed class ThrowingMarkdownRenderer : IMarkdownRenderer
    {
        public int CallCount { get; private set; }

        public MarkupString Render(string markdown)
        {
            CallCount++;
            throw new InvalidOperationException("renderer blew up");
        }
    }

    private static ChatMessage Message(string content, string? id = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        Role = MessageRole.Assistant,
        Content = content
    };

    // The issue's repro: rendering this message threw out of BuildRenderTree and
    // Blazor Server tore down the circuit.
    [Test]
    public void Render_PipeLinesAboveNestingLimit_RendersWithoutThrowing()
    {
        using var ctx = new BunitContext();
        var message = Message(MarkdownInput.PipeLinesWithoutSeparator(40));

        var cut = ctx.Render<MessageBubble>(parameters => parameters
            .Add(p => p.Message, message)
            .Add(p => p.Options, new AgentChatOptions { EnableMarkdown = true }));

        Assert.That(cut.Markup, Does.Contain("Story 40"));
    }

    [Test]
    public void Render_ThrowingRenderer_FallsBackToPlainText()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddSingleton<IMarkdownRenderer>(new ThrowingMarkdownRenderer());
        var message = Message("Hello **world**");

        var cut = ctx.Render<MessageBubble>(parameters => parameters
            .Add(p => p.Message, message)
            .Add(p => p.Options, new AgentChatOptions { EnableMarkdown = true }));

        Assert.Multiple(() =>
        {
            Assert.That(cut.Markup, Does.Contain("bav-text-content"));
            Assert.That(cut.Markup, Does.Contain("Hello **world**"));
            Assert.That(cut.Markup, Does.Not.Contain("bav-markdown-content"));
        });
    }

    // Hosts stream by appending deltas and re-rendering, so a message that already
    // failed must not be handed to the renderer again.
    [Test]
    public void Render_ThrowingRenderer_DoesNotReparseFailedMessage()
    {
        using var ctx = new BunitContext();
        var renderer = new ThrowingMarkdownRenderer();
        ctx.Services.AddSingleton<IMarkdownRenderer>(renderer);
        var id = Guid.NewGuid().ToString();
        var options = new AgentChatOptions { EnableMarkdown = true };

        var cut = ctx.Render<MessageBubble>(parameters => parameters
            .Add(p => p.Message, Message("chunk one", id))
            .Add(p => p.Options, options));

        for (var i = 0; i < 5; i++)
        {
            var delta = i;
            cut.Render(parameters => parameters
                .Add(p => p.Message, Message($"chunk one plus delta {delta}", id))
                .Add(p => p.Options, options));
        }

        Assert.Multiple(() =>
        {
            Assert.That(renderer.CallCount, Is.EqualTo(1));
            Assert.That(cut.Markup, Does.Contain("chunk one plus delta 4"));
        });
    }

    // A different message must still get a chance to render as markdown.
    [Test]
    public void Render_ThrowingRendererThenNewMessage_RetriesMarkdown()
    {
        using var ctx = new BunitContext();
        var renderer = new ThrowingMarkdownRenderer();
        ctx.Services.AddSingleton<IMarkdownRenderer>(renderer);
        var options = new AgentChatOptions { EnableMarkdown = true };

        var cut = ctx.Render<MessageBubble>(parameters => parameters
            .Add(p => p.Message, Message("first"))
            .Add(p => p.Options, options));

        cut.Render(parameters => parameters
            .Add(p => p.Message, Message("second"))
            .Add(p => p.Options, options));

        Assert.That(renderer.CallCount, Is.EqualTo(2));
    }

    [Test]
    public void Render_OptionsMarkdownRenderer_TakesPrecedenceOverContainer()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddSingleton<IMarkdownRenderer>(new ThrowingMarkdownRenderer());
        var options = new AgentChatOptions
        {
            EnableMarkdown = true,
            MarkdownRenderer = new DefaultMarkdownRenderer()
        };

        var cut = ctx.Render<MessageBubble>(parameters => parameters
            .Add(p => p.Message, Message("Hello **world**"))
            .Add(p => p.Options, options));

        Assert.That(cut.Markup, Does.Contain("<strong>world</strong>"));
    }

    [Test]
    public void Render_ValidTable_RendersTable()
    {
        using var ctx = new BunitContext();

        var cut = ctx.Render<MessageBubble>(parameters => parameters
            .Add(p => p.Message, Message(MarkdownInput.ValidTable(200)))
            .Add(p => p.Options, new AgentChatOptions { EnableMarkdown = true }));

        Assert.That(cut.Markup, Does.Contain("<table"));
    }
}
