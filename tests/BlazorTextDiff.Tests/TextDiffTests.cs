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
    public void EqualTextValues_ReuseDiffModels()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content"));
        var models = cut.FindComponents<TextDiffPane>().Select(pane => pane.Instance.Model).ToArray();

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, new string("Old content".ToCharArray()))
            .Add(p => p.NewText, new string("New content".ToCharArray())));

        var panes = cut.FindComponents<TextDiffPane>();
        Assert.Same(models[0], panes[0].Instance.Model);
        Assert.Same(models[1], panes[1].Instance.Model);
    }

    [Fact]
    public void LongInsertion_GroupsHighlightsFromTheDiffEngine()
    {
        var insertedText = new string('x', 2000);
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "prefixsuffix")
            .Add(p => p.NewText, $"prefix{insertedText}suffix"));

        var word = cut.Find(".diff-pane-right .inserted-word");
        Assert.Equal($"prefix{insertedText}suffix", word.TextContent);
        Assert.Equal(insertedText, Assert.Single(word.QuerySelectorAll(".inserted-character")).TextContent);
        Assert.Single(word.QuerySelectorAll("span"));
    }

    [Fact]
    public void NullAndEmptyText_ReuseTheSameComparison()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, (string?)null)
            .Add(p => p.NewText, "New content"));
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.OldText, string.Empty));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
    }

    [Fact]
    public void PresentationChanges_ReuseDiffAndUpdateMarkup()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Line 1")
            .Add(p => p.NewText, "Line 1\nLine 2"));
        var models = cut.FindComponents<TextDiffPane>().Select(pane => pane.Instance.Model).ToArray();
        RenderFragment<DiffStats> header = stats => builder =>
            builder.AddContent(0, $"Added: {stats.LineAdditionCount}");

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CollapseContent, true)
            .Add(p => p.MaxHeight, 450)
            .Add(p => p.Class, "custom-diff")
            .Add(p => p.Header, header)
            .AddUnmatched("data-version", "updated"));

        var panes = cut.FindComponents<TextDiffPane>();
        Assert.Same(models[0], panes[0].Instance.Model);
        Assert.Same(models[1], panes[1].Instance.Model);
        Assert.Equal("updated", cut.Find(".diff-container.custom-diff").GetAttribute("data-version"));
        Assert.Contains("max-height: 450px", cut.Find(".diff-panes").GetAttribute("style"));
        Assert.Equal("Added: 1", cut.Find(".diff-header").TextContent.Trim());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TextChanges_RebuildDiffModels(bool changeOldText)
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content"));
        var models = cut.FindComponents<TextDiffPane>().Select(pane => pane.Instance.Model).ToArray();

        var oldText = changeOldText ? "Updated old content" : "Old content";
        var newText = changeOldText ? "New content" : "Updated new content";
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText));

        var panes = cut.FindComponents<TextDiffPane>();
        Assert.NotSame(models[0], panes[0].Instance.Model);
        Assert.NotSame(models[1], panes[1].Instance.Model);
        Assert.Equal(oldText, cut.Find(".diff-pane-left .line-text").TextContent);
        Assert.Equal(newText, cut.Find(".diff-pane-right .line-text").TextContent);
    }

    [Theory]
    [InlineData(true, false, "hello world")]
    [InlineData(false, true, "Hello  World")]
    public void ComparisonOptions_RebuildDiffWhenEnabledAndDisabled(
        bool ignoreCase, bool ignoreWhiteSpace, string newText)
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Hello World")
            .Add(p => p.NewText, newText));
        var originalModel = cut.FindComponent<TextDiffPane>().Instance.Model;
        Assert.NotEmpty(cut.FindAll("td.line.modified-line"));

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IgnoreCase, ignoreCase)
            .Add(p => p.IgnoreWhiteSpace, ignoreWhiteSpace));

        var ignoredModel = cut.FindComponent<TextDiffPane>().Instance.Model;
        Assert.NotSame(originalModel, ignoredModel);
        Assert.Empty(cut.FindAll("td.line.modified-line"));

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IgnoreCase, false)
            .Add(p => p.IgnoreWhiteSpace, false));

        Assert.NotSame(ignoredModel, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.NotEmpty(cut.FindAll("td.line.modified-line"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData("", null)]
    public void ClearingBothTexts_RemovesPreviousDiffAndHeader(string? oldText, string? newText)
    {
        RenderFragment<DiffStats> header = stats => builder => builder.AddContent(0, "Changes");
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content")
            .Add(p => p.Header, header));
        var originalModel = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText));

        Assert.Empty(cut.FindAll(".diff-pane"));
        Assert.Empty(cut.FindAll(".diff-header"));

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content"));

        Assert.NotSame(originalModel, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Single(cut.FindAll(".diff-header"));
    }

    [Fact]
    public void ParentRerender_PreservesUserExpandedState()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content")
            .Add(p => p.CollapseContent, true));
        cut.Find(".diff-expand-notice").Click();

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CollapseContent, true)
            .Add(p => p.Class, "updated"));

        Assert.Contains("Show less", cut.Find(".diff-expand-notice").TextContent);
        Assert.Contains("max-height: auto", cut.Find(".diff-panes").GetAttribute("style"));

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.CollapseContent, false));
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.CollapseContent, true));

        Assert.Contains("Show more", cut.Find(".diff-expand-notice").TextContent);
        Assert.Contains("max-height: 300px", cut.Find(".diff-panes").GetAttribute("style"));
    }

    [Fact]
    public void DeferDiff_WaitsForBothStagedInputsBeforeFirstComparison()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters.Add(p => p.DeferDiff, true));

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.OldText, "Old content"));
        Assert.Empty(cut.FindComponents<TextDiffPane>());

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.NewText, "New content"));
        Assert.Empty(cut.FindComponents<TextDiffPane>());

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));

        Assert.Equal(2, cut.FindComponents<TextDiffPane>().Count);
        Assert.Equal("Old content", cut.Find(".diff-pane-left .line-text").TextContent);
        Assert.Equal("New content", cut.Find(".diff-pane-right .line-text").TextContent);
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));
        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
    }

    [Fact]
    public void DeferDiff_KeepsLastComparisonVisibleWhileInputsChange()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content"));
        var models = cut.FindComponents<TextDiffPane>().Select(pane => pane.Instance.Model).ToArray();

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.DeferDiff, true)
            .Add(p => p.OldText, "Next old content"));
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.NewText, "Next new content")
            .Add(p => p.Class, "loading"));

        var panes = cut.FindComponents<TextDiffPane>();
        Assert.Same(models[0], panes[0].Instance.Model);
        Assert.Same(models[1], panes[1].Instance.Model);
        Assert.Equal("Old content", cut.Find(".diff-pane-left .line-text").TextContent);
        Assert.Equal("New content", cut.Find(".diff-pane-right .line-text").TextContent);
        Assert.Single(cut.FindAll(".diff-container.loading"));

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));

        Assert.NotSame(models[0], cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Equal("Next old content", cut.Find(".diff-pane-left .line-text").TextContent);
        Assert.Equal("Next new content", cut.Find(".diff-pane-right .line-text").TextContent);
    }

    [Fact]
    public void DeferDiff_UsesLatestComparisonOptionsWhenReleased()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Hello World")
            .Add(p => p.NewText, "hello  World"));
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.DeferDiff, true)
            .Add(p => p.IgnoreCase, true));
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.IgnoreWhiteSpace, true));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.NotEmpty(cut.FindAll("td.line.modified-line"));

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));

        Assert.NotSame(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Empty(cut.FindAll("td.line.modified-line"));
    }

    [Fact]
    public void DeferDiff_ReusesLastComparisonWhenStagedChangesAreReverted()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content"));
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.DeferDiff, true)
            .Add(p => p.OldText, "Temporary content")
            .Add(p => p.IgnoreCase, true));
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.IgnoreCase, false)
            .Add(p => p.DeferDiff, false));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
    }

    [Theory]
    [InlineData(null, "New content", "inserted")]
    [InlineData("", "New content", "inserted")]
    [InlineData("Old content", null, "deleted")]
    [InlineData("Old content", "", "deleted")]
    public void DeferDiff_AllowsComparisonsAgainstEmptyText(
        string? oldText, string? newText, string changeType)
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.DeferDiff, true)
            .Add(p => p.OldText, oldText)
            .Add(p => p.NewText, newText));

        Assert.Empty(cut.FindComponents<TextDiffPane>());

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));

        Assert.Equal(2, cut.FindComponents<TextDiffPane>().Count);
        Assert.Single(cut.FindAll($"td.line.{changeType}-line"));
    }

    [Fact]
    public void DeferDiff_ClearsEmptyInputsOnlyWhenReleased()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Old content")
            .Add(p => p.NewText, "New content"));
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.DeferDiff, true)
            .Add(p => p.OldText, string.Empty)
            .Add(p => p.NewText, (string?)null));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));

        Assert.Empty(cut.FindComponents<TextDiffPane>());
    }
}
