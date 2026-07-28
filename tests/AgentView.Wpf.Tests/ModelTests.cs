using System.ComponentModel;

namespace AgentView.Wpf.Tests;

[TestFixture]
public class ModelTests
{
    [Test]
    public void ChatMessage_HasSafeDefaults()
    {
        var message = new ChatMessage();
        Assert.Multiple(() =>
        {
            Assert.That(message.Id, Is.Empty);
            Assert.That(message.Content, Is.Empty);
            Assert.That(message.Role, Is.EqualTo(MessageRole.User));
            Assert.That(message.RoleLabel, Is.Null);
            Assert.That(message.ToolCalls, Is.Empty);
        });
    }

    [Test]
    public void ChatMessage_RaisesPropertyChanged_OnContentChange()
    {
        var message = new ChatMessage();
        var raised = CollectChanges(message);

        message.Content = "hello";
        message.Content = "hello"; // unchanged value must not raise again

        Assert.That(raised, Is.EqualTo(new[] { nameof(ChatMessage.Content) }));
    }

    [Test]
    public void ToolCall_RaisesPropertyChanged_ForOutputAndState()
    {
        var call = new ToolCall { State = ToolState.Running };
        var raised = CollectChanges(call);

        call.Output = "done";
        call.State = ToolState.Success;

        Assert.That(raised, Is.EqualTo(new[] { nameof(ToolCall.Output), nameof(ToolCall.State) }));
    }

    [Test]
    public void ToolCall_DefaultsToPending()
        => Assert.That(new ToolCall().State, Is.EqualTo(ToolState.Pending));

    [Test]
    public void AgentChatOptions_HasDocumentedDefaults()
    {
        var options = new AgentChatOptions();
        Assert.Multiple(() =>
        {
            Assert.That(options.ShowTimestamps, Is.False);
            Assert.That(options.EnableMarkdown, Is.True);
            Assert.That(options.AutoScroll, Is.True);
            Assert.That(options.ToolCallDisplay, Is.EqualTo(ToolCallDisplayMode.Collapsible));
            Assert.That(options.Theme, Is.EqualTo("dark"));
        });
    }

    [Test]
    public void AgentChatOptions_NullTheme_FallsBackToDark()
    {
        var options = new AgentChatOptions { Theme = null! };
        Assert.That(options.Theme, Is.EqualTo("dark"));
    }

    [Test]
    public void AgentChatOptions_RaisesPropertyChanged_OnThemeChange()
    {
        var options = new AgentChatOptions();
        var raised = CollectChanges(options);

        options.Theme = "light";

        Assert.That(raised, Is.EqualTo(new[] { nameof(AgentChatOptions.Theme) }));
    }

    private static List<string?> CollectChanges(INotifyPropertyChanged source)
    {
        var raised = new List<string?>();
        source.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }
}
