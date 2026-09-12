using Bunit;
using DiffPlex.DiffBuilder.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace BlazorTextDiff.Tests;

public class TextDiffFoldingTests : TestContext
{
    public TextDiffFoldingTests()
    {
        var module = JSInterop.SetupModule("./_content/BlazorTextDiff/js/virtualizedDiff.js");
        var connection = module.SetupModule(call => call.Identifier == "connect");
        connection.SetupVoid("dispose").SetVoidResult();
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void HidingUnchangedLines_PreservesContextAndOriginalLineNumbers(bool virtualize, bool wrapLines)
    {
        var cut = RenderComparison(virtualize, wrapLines);

        cut.WaitForAssertion(() => Assert.Equal(4, cut.FindAll(".diff-hidden-summary").Count));
        Assert.Equal(10, cut.FindAll("tr.diff-row").Count);
        Assert.Equal(new[] { "Show 3 unchanged lines", "Show 4 unchanged lines" },
            cut.FindAll(".diff-pane-left .diff-show-hidden").Select(button => button.TextContent));
        Assert.Equal(new[] { "Line 4", "Line 5", "Line 6" },
            cut.FindAll(".diff-pane-left .line-text").Select(line => line.TextContent));
        Assert.Equal(new[] { "Line 4", "Modified Line 5", "Line 6" },
            cut.FindAll(".diff-pane-right .line-text").Select(line => line.TextContent));
        Assert.Equal(new[] { "4", "5", "6" },
            cut.FindAll(".diff-pane-left tr:not(.diff-hidden-summary) .line-number")
                .Select(number => number.TextContent.Trim()));

        var panes = cut.FindComponents<TextDiffPane>();
        Assert.Same(panes[0].Instance.DisplayRows, panes[1].Instance.DisplayRows);
        if (virtualize)
        {
            var virtualizers = cut.FindComponents<Virtualize<DiffDisplayRow>>();
            Assert.Equal(2, virtualizers.Count);
            Assert.All(virtualizers, component =>
            {
                Assert.Same(panes[0].Instance.DisplayRows, component.Instance.Items);
                Assert.Equal(30, component.Instance.ItemSize);
                Assert.Equal("tr", component.Instance.SpacerElement);
            });
        }
    }

    [Theory]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void RevealingABlock_UpdatesBothPanesWithoutRecomputing(
        bool virtualize, bool wrapLines, bool clickRight)
    {
        var cut = RenderComparison(virtualize, wrapLines);
        cut.WaitForAssertion(() => Assert.Equal(4, cut.FindAll(".diff-show-hidden").Count));
        var models = cut.FindComponents<TextDiffPane>().Select(pane => pane.Instance.Model).ToArray();
        var firstSide = clickRight ? "right" : "left";
        var secondSide = clickRight ? "left" : "right";
        var focusTarget = cut.Find($".diff-pane-{firstSide}").GetAttribute("blazor:elementreference");
        Assert.False(string.IsNullOrEmpty(focusTarget));

        cut.Find($".diff-pane-{firstSide} .diff-show-hidden").Click();

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll(".diff-hidden-summary").Count));
        Assert.Equal(14, cut.FindAll("tr.diff-row").Count);
        Assert.All(cut.FindComponents<TextDiffPane>(), pane =>
            Assert.Contains("Line 1", pane.FindAll(".line-text").Select(line => line.TextContent)));
        var focus = JSInterop.VerifyFocusAsyncInvoke();
        Assert.Equal(focusTarget, Assert.IsType<ElementReference>(focus.Arguments[0]).Id);
        Assert.True(Assert.IsType<bool>(focus.Arguments[1]));

        cut.Find($".diff-pane-{secondSide} .diff-show-hidden").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".diff-hidden-summary")));
        Assert.Equal(20, cut.FindAll("tr.diff-row").Count);
        Assert.Same(models[0], cut.FindComponents<TextDiffPane>()[0].Instance.Model);
        Assert.Same(models[1], cut.FindComponents<TextDiffPane>()[1].Instance.Model);
    }

    [Fact]
    public void PresentationUpdates_KeepRevealedBlocksOpen()
    {
        var cut = RenderComparison();
        cut.Find(".diff-pane-left .diff-show-hidden").Click();
        var rows = cut.FindComponent<TextDiffPane>().Instance.DisplayRows;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Class, "updated")
            .Add(p => p.CollapseContent, true)
            .Add(p => p.MaxHeight, 450));

        Assert.Same(rows, cut.FindComponent<TextDiffPane>().Instance.DisplayRows);
        Assert.Equal(2, cut.FindAll(".diff-hidden-summary").Count);
        Assert.Contains("Line 1", cut.FindAll(".diff-pane-left .line-text").Select(line => line.TextContent));
    }

    [Fact]
    public void HidingAndContextOptions_UpdateVisibilityWithoutRecomputing()
    {
        var cut = RenderComparison();
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.ContextLines, 0));
        Assert.Equal(new[] { "Line 5" }, cut.FindAll(".diff-pane-left .line-text").Select(line => line.TextContent));
        Assert.Equal(new[] { "Show 4 unchanged lines", "Show 5 unchanged lines" },
            cut.FindAll(".diff-pane-left .diff-show-hidden").Select(button => button.TextContent));
        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.ContextLines, int.MaxValue));
        Assert.Empty(cut.FindAll(".diff-hidden-summary"));
        Assert.Equal(20, cut.FindAll(".line-text").Count);
        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.HideUnchangedLines, false)
            .Add(p => p.ContextLines, 1));
        Assert.Null(cut.FindComponent<TextDiffPane>().Instance.DisplayRows);
        Assert.Equal(20, cut.FindAll(".line-text").Count);

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.HideUnchangedLines, true));
        Assert.Equal(4, cut.FindAll(".diff-hidden-summary").Count);
        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void NegativeContext_IsRejected(int contextLines)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenderComponent<TextDiff>(parameters => parameters.Add(p => p.ContextLines, contextLines)));
    }

    [Fact]
    public void IdenticalTexts_RenderOneRevealableBlockPerPane()
    {
        var text = CreateText(10);
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, text)
            .Add(p => p.NewText, text)
            .Add(p => p.HideUnchangedLines, true));

        Assert.Equal(2, cut.FindAll(".diff-hidden-summary").Count);
        Assert.Empty(cut.FindAll(".line-text"));
        Assert.Equal("Show 10 unchanged lines", cut.Find(".diff-pane-left .diff-show-hidden").TextContent);

        cut.Find(".diff-pane-right .diff-show-hidden").Click();

        Assert.Empty(cut.FindAll(".diff-hidden-summary"));
        Assert.Equal(20, cut.FindAll(".line-text").Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmptySide_KeepsAddedOrRemovedRowsVisible(bool emptyOld)
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, emptyOld ? null : CreateText(3))
            .Add(p => p.NewText, emptyOld ? CreateText(3) : null)
            .Add(p => p.HideUnchangedLines, true)
            .Add(p => p.ContextLines, 0));

        Assert.Empty(cut.FindAll(".diff-hidden-summary"));
        Assert.Equal(6, cut.FindAll("tr.diff-row").Count);
    }

    [Fact]
    public void NewOrClearedTexts_ReplaceTheVisibilityState()
    {
        var cut = RenderComparison();
        cut.Find(".diff-show-hidden").Click();

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, CreateText(3))
            .Add(p => p.NewText, CreateText(3)));
        Assert.Equal(2, cut.FindAll(".diff-hidden-summary").Count);
        Assert.Equal("Show 3 unchanged lines", cut.Find(".diff-show-hidden").TextContent);

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, string.Empty)
            .Add(p => p.NewText, (string?)null));
        Assert.Empty(cut.FindComponents<TextDiffPane>());
    }

    [Fact]
    public void DeferredInputs_KeepTheComparisonButAllowVisibilityChanges()
    {
        var cut = RenderComparison();
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.DeferDiff, true)
            .Add(p => p.OldText, "Next original")
            .Add(p => p.NewText, "Next modified")
            .Add(p => p.ContextLines, 0));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Equal(new[] { "Line 5" }, cut.FindAll(".diff-pane-left .line-text").Select(line => line.TextContent));

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));
        Assert.NotSame(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Empty(cut.FindAll(".diff-hidden-summary"));
        Assert.Equal("Next original", cut.Find(".diff-pane-left .line-text").TextContent);
    }

    [Fact]
    public async Task StaleRevealEvent_DoesNotExpandAReplacementProjection()
    {
        var cut = RenderComparison();
        var pane = cut.FindComponent<TextDiffPane>().Instance;
        var oldRow = pane.DisplayRows!.First(row => row.IsHidden);
        var callback = pane.OnShowHiddenLines;

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.ContextLines, 0));
        await cut.InvokeAsync(() => callback.InvokeAsync(oldRow));

        Assert.Equal(2, cut.FindAll(".line-text").Count);
        Assert.Equal("Show 4 unchanged lines", cut.Find(".diff-pane-left .diff-show-hidden").TextContent);
    }

    [Fact]
    public void IgnoreOptionsAndStatistics_UseTheFullDiff()
    {
        RenderFragment<DiffStats> header = stats => builder =>
            builder.AddContent(0, $"Modified: {stats.LineModificationCount}");
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, "Same\nHello World\nSame")
            .Add(p => p.NewText, "Same\nhello  World\nSame")
            .Add(p => p.HideUnchangedLines, true)
            .Add(p => p.ContextLines, 0)
            .Add(p => p.Header, header));

        Assert.Equal("Modified: 1", cut.Find(".diff-header").TextContent.Trim());
        Assert.Equal(2, cut.FindAll(".line-text").Count);

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IgnoreCase, true)
            .Add(p => p.IgnoreWhiteSpace, true));

        Assert.Equal("Modified: 0", cut.Find(".diff-header").TextContent.Trim());
        Assert.Empty(cut.FindAll(".line-text"));
        Assert.Equal("Show 3 unchanged lines", cut.Find(".diff-show-hidden").TextContent);
    }

    [Fact]
    public void ChangesAtBothEnds_KeepTheirContext()
    {
        var text = CreateText(10);
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, text)
            .Add(p => p.NewText, text.Replace("Line 1\n", "Changed 1\n").Replace("Line 10", "Changed 10"))
            .Add(p => p.HideUnchangedLines, true)
            .Add(p => p.ContextLines, 1));

        Assert.Equal(new[] { "Line 1", "Line 2", "Line 9", "Line 10" },
            cut.FindAll(".diff-pane-left .line-text").Select(line => line.TextContent));
        Assert.Equal("Show 6 unchanged lines", cut.Find(".diff-pane-left .diff-show-hidden").TextContent);
    }

    private IRenderedComponent<TextDiff> RenderComparison(bool virtualize = false, bool wrapLines = true)
    {
        var text = CreateText(10);
        return RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, text)
            .Add(p => p.NewText, text.Replace("Line 5", "Modified Line 5"))
            .Add(p => p.HideUnchangedLines, true)
            .Add(p => p.ContextLines, 1)
            .Add(p => p.Virtualize, virtualize)
            .Add(p => p.WrapLines, wrapLines));
    }

    private static string CreateText(int count) =>
        string.Join("\n", Enumerable.Range(1, count).Select(index => $"Line {index}"));
}
