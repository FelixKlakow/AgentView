using System.Windows;

namespace AgentView.Wpf;

/// <summary>Resource keys the view resolves via DynamicResource; override any of them in your resources to re-theme.</summary>
public static class AgentViewResourceKeys
{
    public static ComponentResourceKey BackgroundBrushKey { get; } = new(typeof(AgentChatView), "Background");
    public static ComponentResourceKey SurfaceBrushKey { get; } = new(typeof(AgentChatView), "Surface");
    public static ComponentResourceKey SurfaceStrongBrushKey { get; } = new(typeof(AgentChatView), "SurfaceStrong");
    public static ComponentResourceKey BorderBrushKey { get; } = new(typeof(AgentChatView), "Border");
    public static ComponentResourceKey AccentBrushKey { get; } = new(typeof(AgentChatView), "Accent");
    public static ComponentResourceKey TextBrushKey { get; } = new(typeof(AgentChatView), "Text");
    public static ComponentResourceKey SubduedTextBrushKey { get; } = new(typeof(AgentChatView), "SubduedText");
    public static ComponentResourceKey SuccessBrushKey { get; } = new(typeof(AgentChatView), "Success");
    public static ComponentResourceKey ErrorBrushKey { get; } = new(typeof(AgentChatView), "Error");
    public static ComponentResourceKey MonospaceFontFamilyKey { get; } = new(typeof(AgentChatView), "MonospaceFontFamily");
}
