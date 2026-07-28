using System.Windows;
using System.Windows.Controls;

namespace AgentView.Wpf;

/// <summary>Picks the per-role message template from the visual tree's resources.</summary>
public class MessageTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is not ChatMessage message || container is not FrameworkElement element)
            return base.SelectTemplate(item, container);

        var key = message.Role switch
        {
            MessageRole.User => "AgentView.UserMessageTemplate",
            MessageRole.System => "AgentView.SystemMessageTemplate",
            MessageRole.Tool => "AgentView.ToolMessageTemplate",
            _ => "AgentView.AssistantMessageTemplate"
        };
        return element.TryFindResource(key) as DataTemplate ?? base.SelectTemplate(item, container);
    }
}
