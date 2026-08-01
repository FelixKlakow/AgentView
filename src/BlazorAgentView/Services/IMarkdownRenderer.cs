using Microsoft.AspNetCore.Components;

namespace BlazorAgentView.Services;

public interface IMarkdownRenderer
{
    MarkupString Render(string markdown);
}
