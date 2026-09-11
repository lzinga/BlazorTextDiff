using Bunit;
using DiffPlex.DiffBuilder.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace BlazorTextDiff.Tests;

public class TextDiffVirtualizationTests : TestContext
{
    private readonly BunitJSModuleInterop scrollModule;
    private readonly BunitJSModuleInterop scrollConnection;

    public TextDiffVirtualizationTests()
    {
        scrollModule = JSInterop.SetupModule("./_content/BlazorTextDiff/js/virtualizedDiff.js");
        scrollConnection = scrollModule.SetupModule(call => call.Identifier == "connect");
        scrollConnection.SetupVoid("dispose").SetVoidResult();
    }

    [Fact]
    public void DefaultMode_RendersAllRowsWithoutJavaScript()
    {
        var cut = RenderDiff(virtualize: false);

        Assert.Empty(cut.FindComponents<Virtualize<DiffPiece>>());
        Assert.Empty(cut.FindAll(".diff-virtualized-pane"));
        Assert.Equal(40, cut.FindAll("tr.diff-row").Count);
        Assert.Empty(JSInterop.Invocations);
        Assert.True(cut.Instance.WrapLines);
        Assert.All(cut.FindComponents<TextDiffPane>(), pane => Assert.True(pane.Instance.WrapLines));
    }

    [Fact]
    public void VirtualizedMode_UsesAlignedModelsAndTableSpacers()
    {
        var cut = RenderDiff();
        var panes = cut.FindComponents<TextDiffPane>();
        var virtualizers = cut.FindComponents<Virtualize<DiffPiece>>();

        Assert.Equal(2, virtualizers.Count);
        for (var i = 0; i < virtualizers.Count; i++)
        {
            Assert.Same(panes[i].Instance.Model.Lines, virtualizers[i].Instance.Items);
            Assert.Equal(30, virtualizers[i].Instance.ItemSize);
            Assert.Equal(3, virtualizers[i].Instance.OverscanCount);
            Assert.Equal("tr", virtualizers[i].Instance.SpacerElement);
        }
        Assert.Empty(cut.FindAll("tbody > div"));
        Assert.Empty(cut.FindAll(".diff-expand-notice"));
    }

    [Fact]
    public void VirtualizedMode_ConnectsTheTwoFocusableScrollRegions()
    {
        var cut = RenderDiff();
        var left = cut.Find(".diff-pane-left");
        var right = cut.Find(".diff-pane-right");

        Assert.Equal("region", left.GetAttribute("role"));
        Assert.Equal("region", right.GetAttribute("role"));
        Assert.Equal("Original text", left.GetAttribute("aria-label"));
        Assert.Equal("Modified text", right.GetAttribute("aria-label"));
        Assert.Equal("0", left.GetAttribute("tabindex"));
        Assert.Equal("0", right.GetAttribute("tabindex"));

        var connection = Assert.Single(scrollModule.Invocations, call => call.Identifier == "connect");
        connection.Arguments[0].ShouldBeElementReferenceTo(left);
        connection.Arguments[1].ShouldBeElementReferenceTo(right);
    }

    [Fact]
    public void ViewportChanges_DoNotRecomputeDiffOrReconnectScrolling()
    {
        var cut = RenderDiff();
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.MaxHeight, 700));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.All(cut.FindAll(".diff-virtualized-pane"), pane =>
        {
            Assert.Contains("height: 700px", pane.GetAttribute("style"));
            Assert.Contains("--diff-virtual-row-height: 30px", pane.GetAttribute("style"));
        });
        Assert.Single(scrollModule.Invocations, call => call.Identifier == "connect");
    }

    [Fact]
    public void SwitchingModes_ReusesDiffAndPreservesExpandedState()
    {
        var cut = RenderDiff(virtualize: false);
        var models = cut.FindComponents<TextDiffPane>().Select(pane => pane.Instance.Model).ToArray();
        cut.Find(".diff-expand-notice").Click();

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.Virtualize, true));

        Assert.Equal(2, cut.FindComponents<Virtualize<DiffPiece>>().Count);
        Assert.Empty(cut.FindAll(".diff-expand-notice"));
        Assert.Same(models[0], cut.FindComponents<TextDiffPane>()[0].Instance.Model);
        Assert.Same(models[1], cut.FindComponents<TextDiffPane>()[1].Instance.Model);

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.Virtualize, false));

        Assert.Empty(cut.FindComponents<Virtualize<DiffPiece>>());
        Assert.Equal(40, cut.FindAll("tr.diff-row").Count);
        Assert.Contains("Show less", cut.Find(".diff-expand-notice").TextContent);
        Assert.Same(models[0], cut.FindComponents<TextDiffPane>()[0].Instance.Model);
        cut.WaitForAssertion(() =>
            Assert.Single(scrollConnection.Invocations, call => call.Identifier == "dispose"));
    }

    [Fact]
    public void UpdatedText_RefreshesTheVirtualizedRowsWithoutReconnecting()
    {
        var cut = RenderDiff();
        var originalModel = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, "Original")
            .Add(p => p.NewText, "Updated"));

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("tr.diff-row").Count));
        Assert.NotSame(originalModel, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Equal("Original", cut.Find(".diff-pane-left .line-text").TextContent);
        Assert.Equal("Updated", cut.Find(".diff-pane-right .line-text").TextContent);
        Assert.Single(scrollModule.Invocations, call => call.Identifier == "connect");
    }

    [Fact]
    public void DeferredLoading_PreservesVirtualizedRowsUntilReleased()
    {
        var cut = RenderDiff();
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.DeferDiff, true)
            .Add(p => p.OldText, "Next original"));
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.NewText, "Next modified"));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Equal(40, cut.FindAll("tr.diff-row").Count);

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("tr.diff-row").Count));
        Assert.Equal("Next original", cut.Find(".diff-pane-left .line-text").TextContent);
        Assert.Equal("Next modified", cut.Find(".diff-pane-right .line-text").TextContent);
    }

    [Fact]
    public void InitiallyDeferred_DoesNotInitializeVirtualizationUntilReleased()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.Virtualize, true)
            .Add(p => p.DeferDiff, true)
            .Add(p => p.OldText, CreateText(20))
            .Add(p => p.NewText, CreateText(20)));

        Assert.Empty(cut.FindComponents<Virtualize<DiffPiece>>());
        Assert.Empty(JSInterop.Invocations);

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.DeferDiff, false));

        cut.WaitForAssertion(() => Assert.Equal(40, cut.FindAll("tr.diff-row").Count));
        Assert.Single(scrollModule.Invocations, call => call.Identifier == "connect");
    }

    [Fact]
    public void ClearingAndReloading_ReleasesAndRecreatesScrollSynchronization()
    {
        var cut = RenderDiff();
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, string.Empty)
            .Add(p => p.NewText, (string?)null));

        Assert.Empty(cut.FindComponents<Virtualize<DiffPiece>>());
        cut.WaitForAssertion(() =>
            Assert.Single(scrollConnection.Invocations, call => call.Identifier == "dispose"));

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.OldText, "Old")
            .Add(p => p.NewText, "New"));

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("tr.diff-row").Count));
        Assert.Equal(2, scrollModule.Invocations.Count(call => call.Identifier == "connect"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmptySide_PreservesAlignedImaginaryRows(bool emptyOldText)
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.Virtualize, true)
            .Add(p => p.OldText, emptyOldText ? null : CreateText(3))
            .Add(p => p.NewText, emptyOldText ? CreateText(3) : null));

        cut.WaitForAssertion(() => Assert.Equal(6, cut.FindAll("tr.diff-row").Count));
        Assert.Equal(3, cut.FindAll("td.line.imaginary-line").Count);
        Assert.Equal(3, cut.FindAll(emptyOldText ? "td.line.inserted-line" : "td.line.deleted-line").Count);
    }

    [Fact]
    public void Header_UsesFullDiffStatisticsInVirtualizedMode()
    {
        RenderFragment<DiffStats> header = stats => builder =>
            builder.AddContent(0, $"Added: {stats.LineAdditionCount}");
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.Virtualize, true)
            .Add(p => p.OldText, CreateText(10))
            .Add(p => p.NewText, CreateText(20))
            .Add(p => p.Header, header));

        Assert.Equal("Added: 10", cut.Find(".diff-header").TextContent.Trim());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void VirtualizedMode_RejectsNonpositiveViewportHeights(int maxHeight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RenderComponent<TextDiff>(parameters => parameters
                .Add(p => p.Virtualize, true)
                .Add(p => p.OldText, "Old")
                .Add(p => p.NewText, "New")
                .Add(p => p.MaxHeight, maxHeight)));
    }

    [Fact]
    public void DisablingOneComparison_DoesNotDisconnectAnother()
    {
        var first = RenderDiff();
        var second = RenderDiff();
        first.SetParametersAndRender(parameters => parameters.Add(p => p.Virtualize, false));

        first.WaitForAssertion(() =>
            Assert.Single(scrollConnection.Invocations, call => call.Identifier == "dispose"));
        second.SetParametersAndRender(parameters => parameters.Add(p => p.MaxHeight, 500));
        Assert.Equal(40, second.FindAll("tr.diff-row").Count);
        Assert.Equal(2, scrollModule.Invocations.Count(call => call.Identifier == "connect"));

        second.SetParametersAndRender(parameters => parameters.Add(p => p.Virtualize, false));
        second.WaitForAssertion(() =>
            Assert.Equal(2, scrollConnection.Invocations.Count(call => call.Identifier == "dispose")));
    }

    [Fact]
    public void NonwrappingMode_RendersEveryRowInScrollablePanes()
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, CreateText(20))
            .Add(p => p.NewText, CreateText(20))
            .Add(p => p.WrapLines, false)
            .Add(p => p.CollapseContent, true)
            .Add(p => p.MaxHeight, 450));

        Assert.Empty(cut.FindComponents<Virtualize<DiffPiece>>());
        Assert.Equal(40, cut.FindAll("tr.diff-row").Count);
        Assert.Equal(2, cut.FindAll(".diff-nowrap-pane").Count);
        Assert.All(cut.FindAll(".diff-scroll-pane"), pane =>
        {
            Assert.Equal("0", pane.GetAttribute("tabindex"));
            Assert.Contains("max-height: 450px", pane.GetAttribute("style"));
            Assert.DoesNotContain("--diff-virtual-row-height", pane.GetAttribute("style"));
        });

        cut.Find(".diff-expand-notice").Click();
        Assert.All(cut.FindAll(".diff-scroll-pane"), pane => Assert.Null(pane.GetAttribute("style")));
        Assert.Single(scrollModule.Invocations, call => call.Identifier == "connect");
    }

    [Fact]
    public void SwitchingWrapping_ReusesDiffAndPreservesExpandedState()
    {
        var cut = RenderDiff(virtualize: false);
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;
        cut.Find(".diff-expand-notice").Click();

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.WrapLines, false));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Equal(2, cut.FindAll(".diff-nowrap-pane").Count);
        Assert.Contains("Show less", cut.Find(".diff-expand-notice").TextContent);

        cut.SetParametersAndRender(parameters => parameters.Add(p => p.WrapLines, true));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Empty(cut.FindAll(".diff-scroll-pane"));
        Assert.Equal(40, cut.FindAll("tr.diff-row").Count);
        Assert.Contains("Show less", cut.Find(".diff-expand-notice").TextContent);
        cut.WaitForAssertion(() =>
            Assert.Single(scrollConnection.Invocations, call => call.Identifier == "dispose"));
    }

    [Theory]
    [InlineData("\n", true)]
    [InlineData("\n", false)]
    [InlineData("\r\n", true)]
    [InlineData("\r\n", false)]
    public void ActualLineBreaks_ArePreservedRegardlessOfWrapping(string lineBreak, bool wrapLines)
    {
        var longLine = new string('x', 500);
        var text = $"First line{lineBreak}{longLine}";
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, text)
            .Add(p => p.NewText, text)
            .Add(p => p.WrapLines, wrapLines));

        Assert.All(cut.FindComponents<TextDiffPane>(), pane =>
        {
            var lines = pane.FindAll(".line-text").Select(line => line.TextContent).ToArray();
            Assert.Equal(new[] { "First line", longLine }, lines);
        });
    }

    [Fact]
    public void Virtualization_AlwaysUsesNonwrappingPanes()
    {
        var cut = RenderDiff();
        var model = cut.FindComponent<TextDiffPane>().Instance.Model;

        Assert.All(cut.FindComponents<TextDiffPane>(), pane => Assert.False(pane.Instance.WrapLines));
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.WrapLines, false));

        Assert.Same(model, cut.FindComponent<TextDiffPane>().Instance.Model);
        Assert.Equal(2, cut.FindComponents<Virtualize<DiffPiece>>().Count);
        Assert.Single(scrollModule.Invocations, call => call.Identifier == "connect");
    }

    private IRenderedComponent<TextDiff> RenderDiff(bool virtualize = true)
    {
        var cut = RenderComponent<TextDiff>(parameters => parameters
            .Add(p => p.OldText, CreateText(20))
            .Add(p => p.NewText, CreateText(20))
            .Add(p => p.Virtualize, virtualize)
            .Add(p => p.CollapseContent, true));

        // bUnit's built-in virtualizer emulation exposes all items, without a browser viewport.
        cut.WaitForAssertion(() => Assert.Equal(40, cut.FindAll("tr.diff-row").Count));
        return cut;
    }

    private static string CreateText(int count) =>
        string.Join("\n", Enumerable.Range(1, count).Select(index => $"Line {index}"));
}
