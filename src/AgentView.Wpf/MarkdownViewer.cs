using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AgentView.Wpf;

/// <summary>Shows chat content as selectable text: a read-only RichTextBox for markdown, a read-only TextBox for raw text.</summary>
public class MarkdownViewer : ContentControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(MarkdownViewer),
        new PropertyMetadata("", static (d, _) => ((MarkdownViewer)d).Rebuild()));

    public static readonly DependencyProperty EnableMarkdownProperty = DependencyProperty.Register(
        nameof(EnableMarkdown), typeof(bool), typeof(MarkdownViewer),
        new PropertyMetadata(true, static (d, _) => ((MarkdownViewer)d).Rebuild()));

    /// <summary>
    /// Host-registered renderer for special fenced code blocks: given the fence language and
    /// its code, return a UIElement to embed (e.g. a rendered mermaid diagram) or null for the
    /// default monospace block. The library itself stays dependency-free — hosts bring their
    /// own rendering (a WebView, an SVG engine, …).
    /// </summary>
    public static Func<string, string, UIElement?>? FenceRenderer { get; set; }

    static MarkdownViewer()
    {
        // A default style (not a local value) supplies the themed foreground so that
        // per-template settings — e.g. the user bubble's text brush — can override it.
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MarkdownViewer),
            new FrameworkPropertyMetadata(typeof(MarkdownViewer)));
    }

    public MarkdownViewer()
    {
        Focusable = false;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        Rebuild();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool EnableMarkdown
    {
        get => (bool)GetValue(EnableMarkdownProperty);
        set => SetValue(EnableMarkdownProperty, value);
    }

    private void Rebuild()
    {
        try
        {
            Content = EnableMarkdown ? BuildMarkdownView() : BuildPlainView();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // This runs from a dependency-property callback — once per streaming delta — so an
            // escaping exception ends up in the dispatcher and takes the host application down.
            // Showing the message as plain text is always better than losing the window.
            Debug.WriteLine($"AgentView: building the markdown view failed, falling back to plain text. {ex}");
            Content = BuildPlainView();
        }
    }

    private TextBox BuildPlainView()
    {
        var text = new TextBox
        {
            Text = Text ?? "",
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            TextWrapping = TextWrapping.Wrap,
            Padding = new Thickness(0)
        };
        BindForeground(text);
        return text;
    }

    private RichTextBox BuildMarkdownView()
    {
        var document = new FlowDocument
        {
            PagePadding = new Thickness(0),
            FontFamily = (FontFamily)GetValue(TextElement.FontFamilyProperty),
            FontSize = (double)GetValue(TextElement.FontSizeProperty)
        };
        foreach (var block in MarkdownToInlinesRenderer.Render(Text ?? ""))
            document.Blocks.Add(block);

        var viewer = new RichTextBox
        {
            Document = document,
            IsReadOnly = true,
            IsDocumentEnabled = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        BindForeground(viewer);
        return viewer;
    }

    private void BindForeground(FrameworkElement target)
        => target.SetBinding(TextElement.ForegroundProperty,
            new System.Windows.Data.Binding(nameof(Foreground)) { Source = this });
}
