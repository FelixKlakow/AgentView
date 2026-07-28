using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MdBlock = Markdig.Syntax.Block;
using MdInline = Markdig.Syntax.Inlines.Inline;
using MdTable = Markdig.Extensions.Tables.Table;

namespace AgentView.Wpf;

/// <summary>Walks the Markdig AST and produces native WPF flow-document Blocks/Inlines.</summary>
internal sealed class MarkdownToInlinesRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .UseAutoLinks()
        .Build();

    public static IReadOnlyList<System.Windows.Documents.Block> Render(string markdown)
    {
        var document = Markdown.Parse(markdown ?? "", Pipeline);
        var blocks = new List<System.Windows.Documents.Block>();
        foreach (var block in document)
            AppendBlock(blocks, block);
        return blocks;
    }

    private static void AppendBlock(ICollection<System.Windows.Documents.Block> target, MdBlock block)
    {
        switch (block)
        {
            case HeadingBlock heading:
                target.Add(RenderHeading(heading));
                break;
            case Markdig.Syntax.ParagraphBlock paragraph:
                target.Add(RenderParagraph(paragraph));
                break;
            case FencedCodeBlock code:
                target.Add(RenderCodeBlock(ExtractCode(code)));
                break;
            case CodeBlock code:
                target.Add(RenderCodeBlock(ExtractCode(code)));
                break;
            case Markdig.Syntax.ListBlock list:
                target.Add(RenderList(list));
                break;
            case Markdig.Syntax.QuoteBlock quote:
                target.Add(RenderQuote(quote));
                break;
            case ThematicBreakBlock:
                target.Add(RenderThematicBreak());
                break;
            case MdTable table:
                target.Add(RenderTable(table));
                break;
            case HtmlBlock html:
                target.Add(RenderCodeBlock(ExtractCode(html)));
                break;
            case ContainerBlock container:
                foreach (var child in container)
                    AppendBlock(target, child);
                break;
            case LeafBlock { Inline: not null } leaf:
                var fallback = new Paragraph();
                AppendInlines(fallback.Inlines, leaf.Inline);
                target.Add(fallback);
                break;
        }
    }

    private static Paragraph RenderHeading(HeadingBlock heading)
    {
        var paragraph = new Paragraph
        {
            FontWeight = FontWeights.Bold,
            FontSize = heading.Level switch
            {
                1 => 22,
                2 => 19,
                3 => 16.5,
                _ => 14.5
            },
            Margin = new Thickness(0, heading.Level <= 2 ? 10 : 8, 0, 4)
        };
        if (heading.Inline is not null)
            AppendInlines(paragraph.Inlines, heading.Inline);
        return paragraph;
    }

    private static Paragraph RenderParagraph(Markdig.Syntax.ParagraphBlock block)
    {
        var paragraph = new Paragraph { Margin = new Thickness(0, 2, 0, 6) };
        if (block.Inline is not null)
            AppendInlines(paragraph.Inlines, block.Inline);
        return paragraph;
    }

    private static string ExtractCode(LeafBlock block)
    {
        var lines = block.Lines.Lines;
        if (lines is null)
            return "";
        var slices = new List<string>();
        for (var i = 0; i < block.Lines.Count; i++)
            slices.Add(lines[i].Slice.ToString());
        return string.Join(Environment.NewLine, slices);
    }

    private static BlockUIContainer RenderCodeBlock(string code)
    {
        var text = new TextBox
        {
            Text = code,
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            TextWrapping = TextWrapping.NoWrap,
            Padding = new Thickness(0)
        };
        text.SetResourceReference(TextBox.FontFamilyProperty, AgentViewResourceKeys.MonospaceFontFamilyKey);
        text.SetResourceReference(TextBox.ForegroundProperty, AgentViewResourceKeys.TextBrushKey);

        var scroll = new ScrollViewer
        {
            Content = text,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        var border = new Border
        {
            Child = scroll,
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 6, 8, 6)
        };
        border.SetResourceReference(Border.BackgroundProperty, AgentViewResourceKeys.SurfaceStrongBrushKey);
        return new BlockUIContainer(border) { Margin = new Thickness(0, 4, 0, 6) };
    }

    private static System.Windows.Documents.List RenderList(Markdig.Syntax.ListBlock list)
    {
        var result = new System.Windows.Documents.List
        {
            MarkerStyle = list.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc,
            Margin = new Thickness(0, 2, 0, 6),
            Padding = new Thickness(20, 0, 0, 0)
        };
        foreach (var item in list.OfType<Markdig.Syntax.ListItemBlock>())
        {
            var listItem = new System.Windows.Documents.ListItem();
            var children = new List<System.Windows.Documents.Block>();
            foreach (var child in item)
                AppendBlock(children, child);
            if (children.Count == 0)
                children.Add(new Paragraph());
            foreach (var child in children)
            {
                child.Margin = new Thickness(0, 0, 0, 2);
                listItem.Blocks.Add(child);
            }
            result.ListItems.Add(listItem);
        }
        return result;
    }

    private static Section RenderQuote(Markdig.Syntax.QuoteBlock quote)
    {
        var section = new Section
        {
            BorderThickness = new Thickness(3, 0, 0, 0),
            Padding = new Thickness(10, 2, 0, 2),
            Margin = new Thickness(0, 4, 0, 6)
        };
        section.SetResourceReference(TextElement.ForegroundProperty, AgentViewResourceKeys.SubduedTextBrushKey);
        section.SetResourceReference(System.Windows.Documents.Block.BorderBrushProperty, AgentViewResourceKeys.BorderBrushKey);
        var children = new List<System.Windows.Documents.Block>();
        foreach (var child in quote)
            AppendBlock(children, child);
        foreach (var child in children)
            section.Blocks.Add(child);
        return section;
    }

    private static BlockUIContainer RenderThematicBreak()
    {
        var line = new Border { Height = 1, HorizontalAlignment = HorizontalAlignment.Stretch };
        line.SetResourceReference(Border.BackgroundProperty, AgentViewResourceKeys.BorderBrushKey);
        return new BlockUIContainer(line) { Margin = new Thickness(0, 8, 0, 8) };
    }

    private static System.Windows.Documents.Table RenderTable(MdTable table)
    {
        var result = new System.Windows.Documents.Table
        {
            CellSpacing = 0,
            Margin = new Thickness(0, 4, 0, 6),
            BorderThickness = new Thickness(1, 1, 0, 0)
        };
        result.SetResourceReference(System.Windows.Documents.Block.BorderBrushProperty, AgentViewResourceKeys.BorderBrushKey);
        var columnCount = Math.Max(1, table.ColumnDefinitions.Count);
        for (var i = 0; i < columnCount; i++)
            result.Columns.Add(new TableColumn());

        var group = new TableRowGroup();
        foreach (var row in table.OfType<Markdig.Extensions.Tables.TableRow>())
        {
            var tableRow = new System.Windows.Documents.TableRow();
            if (row.IsHeader)
                tableRow.FontWeight = FontWeights.Bold;
            foreach (var cell in row.OfType<Markdig.Extensions.Tables.TableCell>())
            {
                var content = new List<System.Windows.Documents.Block>();
                foreach (var child in cell)
                    AppendBlock(content, child);
                var tableCell = new System.Windows.Documents.TableCell
                {
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(6, 3, 6, 3)
                };
                tableCell.SetResourceReference(System.Windows.Documents.Block.BorderBrushProperty, AgentViewResourceKeys.BorderBrushKey);
                if (content.Count == 0)
                    content.Add(new Paragraph());
                foreach (var block in content)
                {
                    block.Margin = new Thickness(0);
                    tableCell.Blocks.Add(block);
                }
                tableRow.Cells.Add(tableCell);
            }
            group.Rows.Add(tableRow);
        }
        result.RowGroups.Add(group);
        return result;
    }

    private static void AppendInlines(InlineCollection target, ContainerInline container)
    {
        foreach (var inline in container)
            AppendInline(target, inline);
    }

    private static void AppendInline(InlineCollection target, MdInline inline)
    {
        switch (inline)
        {
            case LiteralInline literal:
                target.Add(new Run(literal.Content.ToString()));
                break;
            case EmphasisInline emphasis:
                var span = new Span();
                if (emphasis.DelimiterCount >= 2)
                    span.FontWeight = FontWeights.Bold;
                if (emphasis.DelimiterCount is 1 or 3)
                    span.FontStyle = FontStyles.Italic;
                AppendInlines(span.Inlines, emphasis);
                target.Add(span);
                break;
            case CodeInline code:
                var run = new Run(code.Content);
                run.SetResourceReference(TextElement.FontFamilyProperty, AgentViewResourceKeys.MonospaceFontFamilyKey);
                run.SetResourceReference(TextElement.BackgroundProperty, AgentViewResourceKeys.SurfaceStrongBrushKey);
                target.Add(run);
                break;
            case LinkInline link:
                AppendLink(target, link);
                break;
            case AutolinkInline autolink:
                var autoHyperlink = CreateHyperlink(autolink.Url);
                autoHyperlink.Inlines.Add(new Run(autolink.Url));
                target.Add(autoHyperlink);
                break;
            case LineBreakInline lineBreak:
                if (lineBreak.IsHard)
                    target.Add(new LineBreak());
                else
                    target.Add(new Run(" "));
                break;
            case HtmlInline html:
                target.Add(new Run(html.Tag));
                break;
            case ContainerInline container:
                AppendInlines(target, container);
                break;
            default:
                var text = inline.ToString();
                if (!string.IsNullOrEmpty(text))
                    target.Add(new Run(text));
                break;
        }
    }

    private static void AppendLink(InlineCollection target, LinkInline link)
    {
        if (link.IsImage)
        {
            // v0.1 renders images as a labeled link instead of downloading content.
            var placeholder = CreateHyperlink(link.Url);
            placeholder.Inlines.Add(new Run($"[image: {link.Title ?? link.Url ?? ""}]"));
            target.Add(placeholder);
            return;
        }

        var hyperlink = CreateHyperlink(link.Url);
        if (link.FirstChild is null)
            hyperlink.Inlines.Add(new Run(link.Url ?? ""));
        else
            AppendInlines(hyperlink.Inlines, link);
        target.Add(hyperlink);
    }

    private static Hyperlink CreateHyperlink(string? url)
    {
        var hyperlink = new Hyperlink();
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            hyperlink.NavigateUri = uri;
        hyperlink.SetResourceReference(TextElement.ForegroundProperty, AgentViewResourceKeys.AccentBrushKey);
        hyperlink.Click += static (sender, _) =>
        {
            if (sender is Hyperlink { NavigateUri: { } target })
                OpenInBrowser(target);
        };
        return hyperlink;
    }

    private static void OpenInBrowser(Uri uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch
        {
            // Opening the browser is best-effort; a broken handler must not crash the host.
        }
    }
}
