using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazorTextDiff
{
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

        public DiffStats(DiffPaneModel diff)
        {
            this.LineAdditionCount = diff.Lines.Count(x => x.Type == ChangeType.Inserted);
            this.LineModificationCount = diff.Lines.Count(x => x.Type == ChangeType.Modified);
            this.LineDeletionCount = diff.Lines.Count(x => x.Type == ChangeType.Deleted);

            this.WordAdditionCount = diff.Lines.SelectMany(i => i.SubPieces).Count(i => i.Type == ChangeType.Inserted);
            this.WordModificationCount = diff.Lines.SelectMany(i => i.SubPieces).Count(i => i.Type == ChangeType.Modified);
            this.WordDeletionCount = diff.Lines.SelectMany(i => i.SubPieces).Count(i => i.Type == ChangeType.Deleted);
        }
    }
}
