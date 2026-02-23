using DiffPlex.DiffBuilder.Model;

namespace BlazorTextDiff.Tests;

public class DiffStatsTests
{
    [Fact]
    public void DefaultConstructor_AllCountsAreZero()
    {
        var stats = new DiffStats();

        Assert.Equal(0, stats.LineAdditionCount);
        Assert.Equal(0, stats.LineModificationCount);
        Assert.Equal(0, stats.LineDeletionCount);
        Assert.Equal(0, stats.WordAdditionCount);
        Assert.Equal(0, stats.WordModificationCount);
        Assert.Equal(0, stats.WordDeletionCount);
    }

    [Fact]
    public void BothPanesNull_AllCountsAreZero()
    {
        var stats = new DiffStats(null, null);

        Assert.Equal(0, stats.LineAdditionCount);
        Assert.Equal(0, stats.LineModificationCount);
        Assert.Equal(0, stats.LineDeletionCount);
        Assert.Equal(0, stats.WordAdditionCount);
        Assert.Equal(0, stats.WordModificationCount);
        Assert.Equal(0, stats.WordDeletionCount);
    }

    [Fact]
    public void EmptyPanes_AllCountsAreZero()
    {
        var oldPane = new DiffPaneModel();
        var newPane = new DiffPaneModel();

        var stats = new DiffStats(oldPane, newPane);

        Assert.Equal(0, stats.LineAdditionCount);
        Assert.Equal(0, stats.LineModificationCount);
        Assert.Equal(0, stats.LineDeletionCount);
    }

    [Fact]
    public void CountsInsertedLinesFromNewPane()
    {
        var newPane = new DiffPaneModel();
        newPane.Lines.Add(new DiffPiece("line1", ChangeType.Inserted));
        newPane.Lines.Add(new DiffPiece("line2", ChangeType.Inserted));
        newPane.Lines.Add(new DiffPiece("line3", ChangeType.Unchanged));

        var stats = new DiffStats(null, newPane);

        Assert.Equal(2, stats.LineAdditionCount);
        Assert.Equal(0, stats.LineModificationCount);
        Assert.Equal(0, stats.LineDeletionCount);
    }

    [Fact]
    public void CountsDeletedLinesFromOldPane()
    {
        var oldPane = new DiffPaneModel();
        oldPane.Lines.Add(new DiffPiece("line1", ChangeType.Deleted));
        oldPane.Lines.Add(new DiffPiece("line2", ChangeType.Unchanged));

        var stats = new DiffStats(oldPane, null);

        Assert.Equal(0, stats.LineAdditionCount);
        Assert.Equal(0, stats.LineModificationCount);
        Assert.Equal(1, stats.LineDeletionCount);
    }

    [Fact]
    public void CountsModifiedLinesFromNewPane()
    {
        var newPane = new DiffPaneModel();
        newPane.Lines.Add(new DiffPiece("line1", ChangeType.Modified));
        newPane.Lines.Add(new DiffPiece("line2", ChangeType.Modified));
        newPane.Lines.Add(new DiffPiece("line3", ChangeType.Modified));

        var stats = new DiffStats(null, newPane);

        Assert.Equal(0, stats.LineAdditionCount);
        Assert.Equal(3, stats.LineModificationCount);
        Assert.Equal(0, stats.LineDeletionCount);
    }

    [Fact]
    public void CountsMixedLineTypes_BothPanes()
    {
        var oldPane = new DiffPaneModel();
        oldPane.Lines.Add(new DiffPiece("deleted", ChangeType.Deleted));
        oldPane.Lines.Add(new DiffPiece("unchanged", ChangeType.Unchanged));
        oldPane.Lines.Add(new DiffPiece(null, ChangeType.Imaginary));

        var newPane = new DiffPaneModel();
        newPane.Lines.Add(new DiffPiece("inserted", ChangeType.Inserted));
        newPane.Lines.Add(new DiffPiece("modified", ChangeType.Modified));
        newPane.Lines.Add(new DiffPiece("unchanged", ChangeType.Unchanged));

        var stats = new DiffStats(oldPane, newPane);

        Assert.Equal(1, stats.LineAdditionCount);
        Assert.Equal(1, stats.LineModificationCount);
        Assert.Equal(1, stats.LineDeletionCount);
    }

    [Fact]
    public void CountsWordLevelChanges_SplitAcrossPanes()
    {
        // Word additions and modifications come from newText pane
        var newModifiedLine = new DiffPiece("modified line", ChangeType.Modified);
        newModifiedLine.SubPieces.Add(new DiffPiece("word1", ChangeType.Inserted));
        newModifiedLine.SubPieces.Add(new DiffPiece("word2", ChangeType.Modified));
        newModifiedLine.SubPieces.Add(new DiffPiece("word3", ChangeType.Unchanged));

        var newPane = new DiffPaneModel();
        newPane.Lines.Add(newModifiedLine);

        // Word deletions come from oldText pane
        var oldModifiedLine = new DiffPiece("modified line", ChangeType.Modified);
        oldModifiedLine.SubPieces.Add(new DiffPiece("word4", ChangeType.Deleted));
        oldModifiedLine.SubPieces.Add(new DiffPiece("word5", ChangeType.Unchanged));

        var oldPane = new DiffPaneModel();
        oldPane.Lines.Add(oldModifiedLine);

        var stats = new DiffStats(oldPane, newPane);

        Assert.Equal(1, stats.WordAdditionCount);
        Assert.Equal(1, stats.WordModificationCount);
        Assert.Equal(1, stats.WordDeletionCount);
    }

    [Fact]
    public void LinesWithNoSubPieces_WordCountsAreZero()
    {
        var newPane = new DiffPaneModel();
        newPane.Lines.Add(new DiffPiece("simple insert", ChangeType.Inserted));

        var oldPane = new DiffPaneModel();
        oldPane.Lines.Add(new DiffPiece("simple delete", ChangeType.Deleted));

        var stats = new DiffStats(oldPane, newPane);

        Assert.Equal(0, stats.WordAdditionCount);
        Assert.Equal(0, stats.WordDeletionCount);
        Assert.Equal(0, stats.WordModificationCount);
    }

    [Fact]
    public void MultipleModifiedLines_AggregatesWordCounts()
    {
        var line1 = new DiffPiece("line1", ChangeType.Modified);
        line1.SubPieces.Add(new DiffPiece("a", ChangeType.Inserted));
        line1.SubPieces.Add(new DiffPiece("b", ChangeType.Inserted));

        var line2 = new DiffPiece("line2", ChangeType.Modified);
        line2.SubPieces.Add(new DiffPiece("c", ChangeType.Modified));

        var newPane = new DiffPaneModel();
        newPane.Lines.Add(line1);
        newPane.Lines.Add(line2);

        var oldLine = new DiffPiece("line1", ChangeType.Modified);
        oldLine.SubPieces.Add(new DiffPiece("d", ChangeType.Deleted));

        var oldPane = new DiffPaneModel();
        oldPane.Lines.Add(oldLine);

        var stats = new DiffStats(oldPane, newPane);

        Assert.Equal(2, stats.WordAdditionCount);
        Assert.Equal(1, stats.WordDeletionCount);
        Assert.Equal(1, stats.WordModificationCount);
    }
}
