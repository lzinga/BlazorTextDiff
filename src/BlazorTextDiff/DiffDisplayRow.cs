namespace BlazorTextDiff;

/// <summary>
/// An aligned source line or a summary of consecutive hidden unchanged lines.
/// </summary>
public sealed class DiffDisplayRow
{
    public int LineIndex { get; }
    public int HiddenLineCount { get; }
    public bool IsHidden => HiddenLineCount > 0;

    internal DiffDisplayRow(int lineIndex, int hiddenLineCount = 0)
    {
        LineIndex = lineIndex;
        HiddenLineCount = hiddenLineCount;
    }
}
