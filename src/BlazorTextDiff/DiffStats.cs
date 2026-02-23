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

    public DiffStats(DiffPaneModel? oldText, DiffPaneModel? newText)
    {
        if (newText?.Lines is not null)
        {
            LineAdditionCount = newText.Lines.Count(x => x.Type == ChangeType.Inserted);
            LineModificationCount = newText.Lines.Count(x => x.Type == ChangeType.Modified);

            WordAdditionCount = newText.Lines
                .Where(line => line.SubPieces?.Any() == true)
                .SelectMany(line => line.SubPieces!)
                .Count(piece => piece.Type == ChangeType.Inserted);

            WordModificationCount = newText.Lines
                .Where(line => line.SubPieces?.Any() == true)
                .SelectMany(line => line.SubPieces!)
                .Count(piece => piece.Type == ChangeType.Modified);
        }

        if (oldText?.Lines is not null)
        {
            LineDeletionCount = oldText.Lines.Count(x => x.Type == ChangeType.Deleted);

            WordDeletionCount = oldText.Lines
                .Where(line => line.SubPieces?.Any() == true)
                .SelectMany(line => line.SubPieces!)
                .Count(piece => piece.Type == ChangeType.Deleted);
        }
    }
}
