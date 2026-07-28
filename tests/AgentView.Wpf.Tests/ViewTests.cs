using System.Windows;
using System.Windows.Controls;

namespace AgentView.Wpf.Tests;

[Apartment(ApartmentState.STA)]
[TestFixture]
public class ViewTests
{
    [Test]
    public void AgentChatView_ProvidesDefaultOptions()
    {
        var view = new AgentChatView();
        Assert.That(view.Options, Is.Not.Null);
        Assert.That(view.Options!.Theme, Is.EqualTo("dark"));
    }

    [Test]
    public void MessageTemplateSelector_PicksTemplateByRole()
    {
        var host = new Grid();
        var userTemplate = new DataTemplate();
        var toolTemplate = new DataTemplate();
        host.Resources.Add("AgentView.UserMessageTemplate", userTemplate);
        host.Resources.Add("AgentView.ToolMessageTemplate", toolTemplate);

        var selector = new MessageTemplateSelector();
        Assert.Multiple(() =>
        {
            Assert.That(selector.SelectTemplate(new ChatMessage { Role = MessageRole.User }, host),
                Is.SameAs(userTemplate));
            Assert.That(selector.SelectTemplate(new ChatMessage { Role = MessageRole.Tool }, host),
                Is.SameAs(toolTemplate));
            Assert.That(selector.SelectTemplate(new ChatMessage { Role = MessageRole.Assistant }, host),
                Is.Null, "no assistant template registered on the host");
        });
    }

    [Test]
    public void MarkdownViewer_SwitchesContentKind_WithEnableMarkdown()
    {
        var viewer = new MarkdownViewer { Text = "**bold**", EnableMarkdown = true };
        Assert.That(viewer.Content, Is.InstanceOf<RichTextBox>());

        viewer.EnableMarkdown = false;
        Assert.That(viewer.Content, Is.InstanceOf<TextBox>());
        Assert.That(((TextBox)viewer.Content).Text, Is.EqualTo("**bold**"));
    }
}
