using Bunit;
using DiffPlex.DiffBuilder.Model;

namespace BlazorTextDiff.Tests;

public class TextDiffPaneTests : TestContext
{
    [Fact]
    public void RendersCorrectPanePositionClass_Left()
    {
        var model = new DiffPaneModel();
        model.Lines.Add(new DiffPiece("line1", ChangeType.Unchanged, 1));

        var cut = RenderComponent<TextDiffPane>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.PanePosition, PanePosition.Left));

        var paneDiv = cut.Find("div");
        Assert.Contains("diff-pane-left", paneDiv.ClassList);
    }

    [Fact]
    public void RendersCorrectPanePositionClass_Right()
    {
        var model = new DiffPaneModel();
        model.Lines.Add(new DiffPiece("line1", ChangeType.Unchanged, 1));

        var cut = RenderComponent<TextDiffPane>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.PanePosition, PanePosition.Right));

        var paneDiv = cut.Find("div");
        Assert.Contains("diff-pane-right", paneDiv.ClassList);
    }

    [Fact]
    public void RendersTableWithCorrectStructure()
    {
        var model = new DiffPaneModel();
        model.Lines.Add(new DiffPiece("first line", ChangeType.Unchanged, 1));
        model.Lines.Add(new DiffPiece("second line", ChangeType.Inserted, 2));

        var cut = RenderComponent<TextDiffPane>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.PanePosition, PanePosition.Left));

        var table = cut.Find("table.diff");
        Assert.NotNull(table);

        var rows = cut.FindAll("tr");
        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void LineNumbersDisplayCorrectly()
    {
        var model = new DiffPaneModel();
        model.Lines.Add(new DiffPiece("line", ChangeType.Unchanged, 42));

        var cut = RenderComponent<TextDiffPane>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.PanePosition, PanePosition.Left));

        var lineNumberCell = cut.Find("td.line-number");
        Assert.Equal("42", lineNumberCell.TextContent);
    }

    [Fact]
    public void NullPosition_RendersNbsp()
    {
        var model = new DiffPaneModel();
        // DiffPiece with null position (e.g., imaginary lines)
        var piece = new DiffPiece(null, ChangeType.Imaginary);
        model.Lines.Add(piece);

        var cut = RenderComponent<TextDiffPane>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.PanePosition, PanePosition.Left));

        var lineNumberCell = cut.Find("td.line-number");
        // The component renders &nbsp; via MarkupString for null positions
        Assert.Contains("&nbsp;", lineNumberCell.InnerHtml);
    }

    [Fact]
    public void AppliesCorrectCssClassesForChangeTypes()
    {
        var model = new DiffPaneModel();
        model.Lines.Add(new DiffPiece("unchanged", ChangeType.Unchanged, 1));
        model.Lines.Add(new DiffPiece("modified", ChangeType.Modified, 2));
        model.Lines.Add(new DiffPiece("inserted", ChangeType.Inserted, 3));
        model.Lines.Add(new DiffPiece("deleted", ChangeType.Deleted, 4));

        var cut = RenderComponent<TextDiffPane>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.PanePosition, PanePosition.Right));

        var lineNumberCells = cut.FindAll("td.line-number");
        Assert.Contains("unchanged", lineNumberCells[0].ClassList);
        Assert.Contains("modified", lineNumberCells[1].ClassList);
        Assert.Contains("inserted", lineNumberCells[2].ClassList);
        Assert.Contains("deleted", lineNumberCells[3].ClassList);

        var lineCells = cut.FindAll("td.line");
        Assert.Contains("unchanged-line", lineCells[0].ClassList);
        Assert.Contains("modified-line", lineCells[1].ClassList);
        Assert.Contains("inserted-line", lineCells[2].ClassList);
        Assert.Contains("deleted-line", lineCells[3].ClassList);
    }

    [Fact]
    public void EmptyModel_RendersTableWithNoRows()
    {
        var model = new DiffPaneModel();

        var cut = RenderComponent<TextDiffPane>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.PanePosition, PanePosition.Left));

        var table = cut.Find("table.diff");
        Assert.NotNull(table);

        var rows = cut.FindAll("tr");
        Assert.Empty(rows);
    }
}
