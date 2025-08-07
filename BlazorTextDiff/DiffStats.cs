using DiffPlex.DiffBuilder.Model;

namespace BlazorTextDiff;

public class DiffStats
{
    public int LineAdditionCount { get; set; }
    public int LineModificationCount { get; set; }
    public int LineDeletionCount { get; set; }

    public int WordAdditionCount { get; set; }
    public int WordModificationCount { get; set; }
    public int WordDeletionCount { get; set; }

    public DiffStats()
    {
    }

    public DiffStats(DiffPaneModel? diff)
    {
        if (diff?.Lines is null) return;

        LineAdditionCount = diff.Lines.Count(x => x.Type == ChangeType.Inserted);
        LineModificationCount = diff.Lines.Count(x => x.Type == ChangeType.Modified);
        LineDeletionCount = diff.Lines.Count(x => x.Type == ChangeType.Deleted);

        // Handle SubPieces safely - some lines might not have SubPieces
        WordAdditionCount = diff.Lines
            .Where(line => line.SubPieces?.Any() == true)
            .SelectMany(line => line.SubPieces!)
            .Count(piece => piece.Type == ChangeType.Inserted);
            
        WordModificationCount = diff.Lines
            .Where(line => line.SubPieces?.Any() == true)
            .SelectMany(line => line.SubPieces!)
            .Count(piece => piece.Type == ChangeType.Modified);
            
        WordDeletionCount = diff.Lines
            .Where(line => line.SubPieces?.Any() == true)
            .SelectMany(line => line.SubPieces!)
            .Count(piece => piece.Type == ChangeType.Deleted);
    }
}
