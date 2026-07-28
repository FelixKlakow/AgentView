using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AgentView.Wpf;

/// <summary>Agent-conversation view: virtualized message list with per-role bubbles, tool-call cards, markdown and auto-scroll.</summary>
[TemplatePart(Name = ItemsPartName, Type = typeof(ItemsControl))]
public class AgentChatView : Control
{
    private const string ItemsPartName = "PART_Items";
    private const string ScrollViewerPartName = "PART_ScrollViewer";

    public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register(
        nameof(Messages), typeof(IEnumerable<ChatMessage>), typeof(AgentChatView), new PropertyMetadata(null));

    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
        nameof(Options), typeof(AgentChatOptions), typeof(AgentChatView),
        new PropertyMetadata(null, static (d, e) => ((AgentChatView)d).OnOptionsChanged(e)));

    private ItemsControl? _items;
    private ScrollViewer? _scrollViewer;
    private ResourceDictionary? _lightDictionary;
    private bool _pinnedToBottom = true;

    static AgentChatView()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(AgentChatView),
            new FrameworkPropertyMetadata(typeof(AgentChatView)));

    public AgentChatView()
        => SetCurrentValue(OptionsProperty, new AgentChatOptions());

    /// <summary>The conversation; use an ObservableCollection for streaming append.</summary>
    public IEnumerable<ChatMessage>? Messages
    {
        get => (IEnumerable<ChatMessage>?)GetValue(MessagesProperty);
        set => SetValue(MessagesProperty, value);
    }

    public AgentChatOptions? Options
    {
        get => (AgentChatOptions?)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_scrollViewer is not null)
            _scrollViewer.ScrollChanged -= OnScrollChanged;
        _scrollViewer = null;

        base.OnApplyTemplate();
        _items = GetTemplateChild(ItemsPartName) as ItemsControl;

        // The ScrollViewer lives inside the ItemsControl's own template (required for
        // virtualization), so it is only reachable after that template has been applied.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, AttachScrollViewer);
    }

    private void AttachScrollViewer()
    {
        if (_items is null)
            return;
        _items.ApplyTemplate();
        _scrollViewer = _items.Template?.FindName(ScrollViewerPartName, _items) as ScrollViewer
                        ?? FindDescendantScrollViewer(_items);
        if (_scrollViewer is null)
            return;
        _scrollViewer.ScrollChanged += OnScrollChanged;
        if (Options?.AutoScroll != false)
            _scrollViewer.ScrollToEnd();
    }

    private static ScrollViewer? FindDescendantScrollViewer(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is ScrollViewer viewer)
                return viewer;
            if (FindDescendantScrollViewer(child) is { } nested)
                return nested;
        }
        return null;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_scrollViewer is not { } viewer)
            return;
        if (e.ExtentHeightChange == 0)
        {
            // A user-driven scroll: re-evaluate whether the view is pinned to the bottom.
            _pinnedToBottom = viewer.VerticalOffset >= viewer.ScrollableHeight - 4;
        }
        else if (_pinnedToBottom && Options?.AutoScroll != false)
        {
            viewer.ScrollToEnd();
        }
    }

    private void OnOptionsChanged(DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is AgentChatOptions oldOptions)
            oldOptions.PropertyChanged -= OnOptionPropertyChanged;
        if (e.NewValue is AgentChatOptions newOptions)
            newOptions.PropertyChanged += OnOptionPropertyChanged;
        ApplyTheme();
    }

    private void OnOptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null or nameof(AgentChatOptions.Theme))
            ApplyTheme();
    }

    private void ApplyTheme()
    {
        var light = string.Equals(Options?.Theme, "light", StringComparison.OrdinalIgnoreCase);
        if (light && _lightDictionary is null)
        {
            _lightDictionary = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/AgentView.Wpf;component/Themes/Light.xaml")
            };
            Resources.MergedDictionaries.Add(_lightDictionary);
        }
        else if (!light && _lightDictionary is not null)
        {
            Resources.MergedDictionaries.Remove(_lightDictionary);
            _lightDictionary = null;
        }
    }
}
