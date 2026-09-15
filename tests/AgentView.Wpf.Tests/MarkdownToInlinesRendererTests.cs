using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AgentView.Wpf.Tests;

[Apartment(ApartmentState.STA)]
[TestFixture]
public class MarkdownToInlinesRendererTests
{
    [Test]
    public void EmptyInput_YieldsNoBlocks()
        => Assert.That(MarkdownToInlinesRenderer.Render(""), Is.Empty);

    [Test]
    public void Heading_BecomesBoldParagraph_WithLevelSize()
    {
        var blocks = MarkdownToInlinesRenderer.Render("# Title");

        Assert.That(blocks, Has.Count.EqualTo(1));
        var paragraph = (Paragraph)blocks[0];
        Assert.Multiple(() =>
        {
            Assert.That(paragraph.FontWeight, Is.EqualTo(FontWeights.Bold));
            Assert.That(paragraph.FontSize, Is.EqualTo(22));
            Assert.That(FlattenText(paragraph.Inlines), Is.EqualTo("Title"));
        });
    }

    [Test]
    public void SecondLevelHeading_IsSmallerThanFirst()
    {
        var h1 = (Paragraph)MarkdownToInlinesRenderer.Render("# A")[0];
        var h2 = (Paragraph)MarkdownToInlinesRenderer.Render("## A")[0];
        Assert.That(h2.FontSize, Is.LessThan(h1.FontSize));
    }

    [Test]
    public void BoldAndItalic_BecomeStyledSpans()
    {
        var paragraph = (Paragraph)MarkdownToInlinesRenderer.Render("plain **bold** and *italic*")[0];

        var spans = paragraph.Inlines.OfType<Span>().Where(s => s is not Hyperlink).ToList();
        Assert.That(spans, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(spans[0].FontWeight, Is.EqualTo(FontWeights.Bold));
            Assert.That(FlattenText(spans[0].Inlines), Is.EqualTo("bold"));
            Assert.That(spans[1].FontStyle, Is.EqualTo(FontStyles.Italic));
            Assert.That(FlattenText(spans[1].Inlines), Is.EqualTo("italic"));
        });
    }

    [Test]
    public void InlineCode_KeepsItsText()
    {
        var paragraph = (Paragraph)MarkdownToInlinesRenderer.Render("run `dotnet build` now")[0];
        Assert.That(FlattenText(paragraph.Inlines), Is.EqualTo("run dotnet build now"));
    }

    [Test]
    public void FencedCodeBlock_BecomesEmbeddedReadOnlyTextBox()
    {
        var blocks = MarkdownToInlinesRenderer.Render("```csharp\nvar x = 1;\nvar y = 2;\n```");

        var container = (BlockUIContainer)blocks.Single();
        var textBox = FindDescendant<TextBox>((FrameworkElement)container.Child);
        Assert.That(textBox, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(textBox!.Text, Is.EqualTo($"var x = 1;{Environment.NewLine}var y = 2;"));
            Assert.That(textBox.IsReadOnly, Is.True);
            Assert.That(textBox.TextWrapping, Is.EqualTo(TextWrapping.NoWrap));
        });
    }

    [Test]
    public void BulletList_WithNesting_ProducesNestedLists()
    {
        var blocks = MarkdownToInlinesRenderer.Render("- one\n- two\n  - nested\n- three");

        var list = (System.Windows.Documents.List)blocks.Single();
        Assert.Multiple(() =>
        {
            Assert.That(list.MarkerStyle, Is.EqualTo(TextMarkerStyle.Disc));
            Assert.That(list.ListItems.Count, Is.EqualTo(3));
        });
        var second = list.ListItems.ElementAt(1);
        var nested = second.Blocks.OfType<System.Windows.Documents.List>().Single();
        Assert.That(FlattenText(((Paragraph)nested.ListItems.First().Blocks.FirstBlock).Inlines),
            Is.EqualTo("nested"));
    }

    [Test]
    public void OrderedList_UsesDecimalMarkers()
    {
        var list = (System.Windows.Documents.List)MarkdownToInlinesRenderer.Render("1. first\n2. second")[0];
        Assert.That(list.MarkerStyle, Is.EqualTo(TextMarkerStyle.Decimal));
    }

    [Test]
    public void Link_BecomesHyperlink_WithNavigateUri()
    {
        var paragraph = (Paragraph)MarkdownToInlinesRenderer.Render("see [the docs](https://example.com/docs)")[0];

        var hyperlink = paragraph.Inlines.OfType<Hyperlink>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(hyperlink.NavigateUri, Is.EqualTo(new Uri("https://example.com/docs")));
            Assert.That(FlattenText(hyperlink.Inlines), Is.EqualTo("the docs"));
        });
    }

    [Test]
    public void BlockQuote_BecomesSectionWithLeftBorder()
    {
        var blocks = MarkdownToInlinesRenderer.Render("> quoted wisdom");

        var section = (Section)blocks.Single();
        Assert.Multiple(() =>
        {
            Assert.That(section.BorderThickness.Left, Is.GreaterThan(0));
            Assert.That(FlattenText(((Paragraph)section.Blocks.FirstBlock).Inlines), Is.EqualTo("quoted wisdom"));
        });
    }

    [Test]
    public void PipeTable_BecomesFlowDocumentTable_WithBoldHeader()
    {
        var blocks = MarkdownToInlinesRenderer.Render("| A | B |\n| - | - |\n| 1 | 2 |");

        var table = (System.Windows.Documents.Table)blocks.Single();
        var rows = table.RowGroups[0].Rows;
        Assert.Multiple(() =>
        {
            Assert.That(table.Columns, Has.Count.EqualTo(2));
            Assert.That(rows, Has.Count.EqualTo(2));
            Assert.That(rows[0].FontWeight, Is.EqualTo(FontWeights.Bold));
            Assert.That(rows[0].Cells, Has.Count.EqualTo(2));
            Assert.That(FlattenText(((Paragraph)rows[1].Cells[1].Blocks.FirstBlock).Inlines), Is.EqualTo("2"));
        });
    }

    [Test]
    public void ThematicBreak_BecomesHorizontalRule()
    {
        var blocks = MarkdownToInlinesRenderer.Render("above\n\n---\n\nbelow");
        Assert.Multiple(() =>
        {
            Assert.That(blocks, Has.Count.EqualTo(3));
            Assert.That(blocks[1], Is.InstanceOf<BlockUIContainer>());
        });
    }

    [Test]
    public void HardLineBreak_BecomesLineBreakInline()
    {
        var paragraph = (Paragraph)MarkdownToInlinesRenderer.Render("first  \nsecond")[0];
        Assert.That(paragraph.Inlines.OfType<LineBreak>().Count(), Is.EqualTo(1));
    }

    [Test]
    public void SoftLineBreak_BecomesSpace()
    {
        var paragraph = (Paragraph)MarkdownToInlinesRenderer.Render("first\nsecond")[0];
        Assert.That(FlattenText(paragraph.Inlines), Is.EqualTo("first second"));
    }

    // Markdig keeps one nested delimiter inline per "|", so a long block of pipe-delimited
    // lines — the shape of tool output agents paste verbatim — nests hundreds of levels deep.
    [Test]
    public void PipeLinesWithoutSeparatorRow_RenderWithoutThrowing()
    {
        var markdown = string.Join("\n", Enumerable.Range(1, 40)
            .Select(i => $"#{i} | AreaPath=TPA\\Line\\OIB | State=Done | Title=Story {i}"));

        IReadOnlyList<Block> blocks = null!;
        Assert.DoesNotThrow(() => blocks = MarkdownToInlinesRenderer.Render(markdown));
        Assert.That(FlattenBlocks(blocks), Does.Contain("Story 40"));
    }

    // Nesting deeper than the walker's cap is flattened instead of recursed over: unbounded
    // recursion here would overflow the stack, which no try/catch can recover from.
    [Test]
    public void DeeplyNestedInput_IsFlattened_AndKeepsItsText()
    {
        var markdown = string.Concat(Enumerable.Repeat("> ", 200)) + "still readable";

        IReadOnlyList<Block> blocks = null!;
        Assert.DoesNotThrow(() => blocks = MarkdownToInlinesRenderer.Render(markdown));
        Assert.That(FlattenBlocks(blocks), Does.Contain("still readable"));
    }

    [Test]
    public void DeeplyNestedEmphasis_IsFlattened_AndKeepsItsText()
    {
        var markdown = string.Concat(Enumerable.Repeat("*", 200)) + "still readable"
                       + string.Concat(Enumerable.Repeat("*", 200));

        IReadOnlyList<Block> blocks = null!;
        Assert.DoesNotThrow(() => blocks = MarkdownToInlinesRenderer.Render(markdown));
        Assert.That(FlattenBlocks(blocks), Does.Contain("still readable"));
    }

    [Test]
    public void ThrowingFenceRenderer_FallsBackToDefaultCodeBlock()
    {
        MarkdownViewer.FenceRenderer = static (_, _) => throw new InvalidOperationException("host renderer blew up");
        try
        {
            IReadOnlyList<Block> blocks = null!;
            Assert.DoesNotThrow(() => blocks = MarkdownToInlinesRenderer.Render("```mermaid\ngraph TD;\n```"));

            var container = (BlockUIContainer)blocks.Single();
            var textBox = FindDescendant<TextBox>((FrameworkElement)container.Child);
            Assert.That(textBox?.Text, Does.Contain("graph TD;"));
        }
        finally
        {
            MarkdownViewer.FenceRenderer = null;
        }
    }

    [Test]
    public void RenderPlainText_KeepsTheTextInASingleParagraph()
    {
        var blocks = MarkdownToInlinesRenderer.RenderPlainText("a | b\nc | d");

        var paragraph = (Paragraph)blocks.Single();
        Assert.That(FlattenText(paragraph.Inlines), Is.EqualTo("a | b\nc | d"));
    }

    [Test]
    public void RenderPlainText_OfEmptyInput_YieldsNoBlocks()
        => Assert.That(MarkdownToInlinesRenderer.RenderPlainText(""), Is.Empty);

    private static string FlattenBlocks(IEnumerable<Block> blocks)
        => string.Join("\n", blocks.Select(FlattenBlock));

    private static string FlattenBlock(Block block) => block switch
    {
        Paragraph paragraph => FlattenText(paragraph.Inlines),
        Section section => FlattenBlocks(section.Blocks),
        System.Windows.Documents.List list => FlattenBlocks(list.ListItems.SelectMany(item => item.Blocks)),
        System.Windows.Documents.Table table => FlattenBlocks(
            table.RowGroups.SelectMany(group => group.Rows)
                .SelectMany(row => row.Cells)
                .SelectMany(cell => cell.Blocks)),
        BlockUIContainer { Child: FrameworkElement child } => FindDescendant<TextBox>(child)?.Text ?? "",
        _ => ""
    };

    private static string FlattenText(InlineCollection inlines)
        => string.Concat(inlines.Select(FlattenText));

    private static string FlattenText(Inline inline) => inline switch
    {
        Run run => run.Text,
        Span span => string.Concat(span.Inlines.Select(FlattenText)),
        LineBreak => "\n",
        _ => ""
    };

    private static T? FindDescendant<T>(FrameworkElement root) where T : FrameworkElement
    {
        if (root is T match)
            return match;
        return root switch
        {
            Border border when border.Child is FrameworkElement child => FindDescendant<T>(child),
            ContentControl { Content: FrameworkElement content } => FindDescendant<T>(content),
            _ => null
        };
    }
}
