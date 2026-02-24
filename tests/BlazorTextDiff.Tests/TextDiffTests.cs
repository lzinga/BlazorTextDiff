using Bunit;
using Microsoft.AspNetCore.Components;

namespace BlazorTextDiff.Tests;

public class TextDiffTests : TestContext
{

    [Fact]
    public void RendersWithIdenticalText_ShowsNoChanges()
    {
        var text = "Hello World\nLine 2";

        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, text)
            .Add(p => p.NewText, text));

        // Should render two panes (old and new)
        var panes = cut.FindAll(".diff-pane");
        Assert.Equal(2, panes.Count);

        // All lines should be unchanged - no modified/inserted/deleted classes on line cells
        var modifiedLines = cut.FindAll("td.line.modified-line");
        var insertedLines = cut.FindAll("td.line.inserted-line");
        var deletedLines = cut.FindAll("td.line.deleted-line");
        Assert.Empty(modifiedLines);
        Assert.Empty(insertedLines);
        Assert.Empty(deletedLines);
    }

    [Fact]
    public void RendersWithDifferentText_ShowsChanges()
    {
        var oldText = "Hello World";
        var newText = "Hello Blazor";

        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText));

        // Should render two panes
        var panes = cut.FindAll(".diff-pane");
        Assert.Equal(2, panes.Count);

        // Should have modified lines
        var modifiedLineNumbers = cut.FindAll("td.line-number.modified");
        Assert.NotEmpty(modifiedLineNumbers);
    }

    [Fact]
    public void NullOldAndNewText_DoesNotRenderPanes()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, (string?)null)
            .Add(p => p.NewText, (string?)null));

        var panes = cut.FindAll(".diff-pane");
        Assert.Empty(panes);
    }

    [Fact]
    public void EmptyOldAndNewText_DoesNotRenderPanes()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, string.Empty)
            .Add(p => p.NewText, string.Empty));

        var panes = cut.FindAll(".diff-pane");
        Assert.Empty(panes);
    }

    [Fact]
    public void RendersWithOnlyNewText()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, (string?)null)
            .Add(p => p.NewText, "New content"));

        var panes = cut.FindAll(".diff-pane");
        Assert.Equal(2, panes.Count);
    }

    [Fact]
    public void RendersWithOnlyOldText()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, (string?)null));

        var panes = cut.FindAll(".diff-pane");
        Assert.Equal(2, panes.Count);
    }

    [Fact]
    public void CollapseContent_AppliesMaxHeightStyle()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Hello")
            .Add(p => p.NewText, "World")
            .Add(p => p.CollapseContent, true)
            .Add(p => p.MaxHeight, 500));

        var panesDiv = cut.Find(".diff-panes");
        Assert.Contains("max-height: 500px", panesDiv.GetAttribute("style"));
        Assert.Contains("overflow: auto", panesDiv.GetAttribute("style"));
    }

    [Fact]
    public void NoCollapse_UsesAutoMaxHeight()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Hello")
            .Add(p => p.NewText, "World")
            .Add(p => p.CollapseContent, false));

        var panesDiv = cut.Find(".diff-panes");
        Assert.Contains("max-height: auto", panesDiv.GetAttribute("style"));
    }

    [Fact]
    public void IgnoreCase_AffectsDiffOutput()
    {
        var oldText = "Hello World";
        var newText = "hello world";

        // With IgnoreCase = false, should detect changes
        var cutCaseSensitive = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText)
            .Add(p => p.IgnoreCase, false));

        var modifiedWithCase = cutCaseSensitive.FindAll("td.line-number.modified");

        // With IgnoreCase = true, should show no changes
        var cutCaseInsensitive = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText)
            .Add(p => p.IgnoreCase, true));

        var modifiedIgnoreCase = cutCaseInsensitive.FindAll("td.line-number.modified");

        // Case-sensitive should detect more differences than case-insensitive
        Assert.True(modifiedWithCase.Count > modifiedIgnoreCase.Count,
            $"Case-sensitive should find more modifications ({modifiedWithCase.Count}) than case-insensitive ({modifiedIgnoreCase.Count})");
    }

    [Fact]
    public void IgnoreWhiteSpace_AffectsDiffOutput()
    {
        var oldText = "Hello World";
        var newText = "Hello  World";

        // With IgnoreWhiteSpace = false, should detect the space change
        var cutWhitespaceAware = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText)
            .Add(p => p.IgnoreWhiteSpace, false));

        var modifiedWithWs = cutWhitespaceAware.FindAll("td.line-number.modified");

        // With IgnoreWhiteSpace = true, should show no changes
        var cutWhitespaceIgnored = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText)
            .Add(p => p.IgnoreWhiteSpace, true));

        var modifiedIgnoreWs = cutWhitespaceIgnored.FindAll("td.line-number.modified");

        Assert.True(modifiedWithWs.Count >= modifiedIgnoreWs.Count,
            $"Whitespace-aware should find at least as many modifications ({modifiedWithWs.Count}) as whitespace-ignored ({modifiedIgnoreWs.Count})");
    }

    [Fact]
    public void Header_RenderFragmentIsDisplayed()
    {
        RenderFragment<DiffStats> headerTemplate = stats => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "test-header");
            builder.AddContent(2, $"Modifications: {stats.LineModificationCount}");
            builder.CloseElement();
        };

        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old")
            .Add(p => p.NewText, "New")
            .Add(p => p.Header, headerTemplate));

        var header = cut.Find(".diff-header");
        Assert.NotNull(header);

        var testHeader = cut.Find(".test-header");
        Assert.NotNull(testHeader);
        Assert.Contains("Modifications:", testHeader.TextContent);
    }

    [Fact]
    public void WithoutHeader_NoHeaderDivRendered()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old")
            .Add(p => p.NewText, "New"));

        var headers = cut.FindAll(".diff-header");
        Assert.Empty(headers);
    }

    [Fact]
    public void MultiLineText_RendersAllLines()
    {
        var oldText = "Line 1\nLine 2\nLine 3";
        var newText = "Line 1\nModified Line 2\nLine 3\nLine 4";

        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText));

        // Right pane should have 4 lines (plus possible imaginary lines)
        var rightPaneDiv = cut.Find(".diff-pane-right");
        Assert.NotNull(rightPaneDiv);

        var rightRows = rightPaneDiv.QuerySelectorAll("tr");
        Assert.True(rightRows.Length >= 4, $"Expected at least 4 rows in right pane, got {rightRows.Length}");
    }

    [Fact]
    public void HideUnchangedLines_HidesExcessContext()
    {
        // 10 lines, change at line 5
        var oldText = string.Join("\n", Enumerable.Range(1, 10).Select(i => $"Line {i}"));
        var newText = oldText.Replace("Line 5", "Modified Line 5");

        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText)
            .Add(p => p.HideUnchangedLines, true)
            .Add(p => p.ContextLines, 1));

        // Line 5 is changed.
        // Context 1: Lines 4, 5, 6 should be visible.
        // Lines 1, 2, 3 should be hidden (3 lines).
        // Lines 7, 8, 9, 10 should be hidden (4 lines).

        var hiddenSummaries = cut.FindAll(".diff-hidden-summary");
        Assert.Equal(4, hiddenSummaries.Count); // One at top, one at bottom for each side

        Assert.Contains("3 lines hidden", hiddenSummaries[0].TextContent);
        Assert.Contains("4 lines hidden", hiddenSummaries[1].TextContent);

        // Check that visible lines are indeed shown
        var lineTexts = cut.FindAll(".line-text").Select(el => el.TextContent).ToList();
        Assert.Contains("Line 4", lineTexts);
        Assert.Contains("Modified Line 5", lineTexts);
        Assert.Contains("Line 6", lineTexts);
        Assert.DoesNotContain("Line 1", lineTexts);
        Assert.DoesNotContain("Line 10", lineTexts);
    }
}
