using Bunit;
using DiffPlex.DiffBuilder.Model;

namespace BlazorTextDiff.Tests;

public class TextDiffLineTests : TestContext
{
    [Fact]
    public void UnchangedLine_RendersPlainText()
    {
        var piece = new DiffPiece("Hello World", ChangeType.Unchanged);

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        cut.MarkupMatches("Hello World");
    }

    [Fact]
    public void InsertedLine_RendersPlainText()
    {
        var piece = new DiffPiece("New line", ChangeType.Inserted);

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        cut.MarkupMatches("New line");
    }

    [Fact]
    public void DeletedLine_RendersPlainText()
    {
        var piece = new DiffPiece("Old line", ChangeType.Deleted);

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        cut.MarkupMatches("Old line");
    }

    [Fact]
    public void ModifiedLine_RendersSubPiecesWithCssClasses()
    {
        // "same " is unchanged, "changed" is modified (a word), " " is unchanged whitespace, "added" is inserted (a word)
        var piece = new DiffPiece("modified", ChangeType.Modified);
        piece.SubPieces.Add(new DiffPiece("s", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("a", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("m", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("e", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece(" ", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("c", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("h", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("a", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("n", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("g", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("e", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("d", ChangeType.Modified));

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        // "same" is all unchanged → no span. "changed" is fully changed → character-level span
        Assert.Contains("same", cut.Markup);
        var spans = cut.FindAll("span");
        Assert.Single(spans);
        Assert.Contains("modified-character", spans[0].ClassList);
        Assert.Equal("changed", spans[0].TextContent);
    }

    [Fact]
    public void ModifiedLine_PartiallyChangedWord_RendersWordAndCharacterSpans()
    {
        // "Program" unchanged + "m" inserted + "ing" unchanged = word "Programming" partially changed
        var piece = new DiffPiece("modified", ChangeType.Modified);
        piece.SubPieces.Add(new DiffPiece("P", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("r", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("o", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("g", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("r", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("a", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("m", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("m", ChangeType.Inserted));
        piece.SubPieces.Add(new DiffPiece("i", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("n", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("g", ChangeType.Unchanged));

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        // Word-level span wraps the whole word, character-level span wraps the inserted "m"
        var wordSpan = cut.Find("span.inserted-word");
        Assert.NotNull(wordSpan);
        Assert.Equal("Programming", wordSpan.TextContent);

        var charSpan = cut.Find("span.inserted-character");
        Assert.NotNull(charSpan);
        Assert.Equal("m", charSpan.TextContent);
    }

    [Fact]
    public void ModifiedLine_SkipsImaginarySubPieces()
    {
        var piece = new DiffPiece("mod", ChangeType.Modified);
        piece.SubPieces.Add(new DiffPiece("v", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("i", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("s", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece(null, ChangeType.Imaginary));
        piece.SubPieces.Add(new DiffPiece(" ", ChangeType.Unchanged));
        piece.SubPieces.Add(new DiffPiece("n", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("e", ChangeType.Modified));
        piece.SubPieces.Add(new DiffPiece("w", ChangeType.Modified));

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        // Imaginary is skipped; "vis" is unchanged (no span), "new" is fully changed (character span)
        Assert.Contains("vis", cut.Markup);
        var changedSpan = cut.Find("span.modified-character");
        Assert.Equal("new", changedSpan.TextContent);

        // Imaginary text should not appear
        Assert.DoesNotContain("Imaginary", cut.Markup);
    }

    [Fact]
    public void NullOrEmptyText_RendersNothing()
    {
        var piece = new DiffPiece(null, ChangeType.Imaginary);

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        cut.MarkupMatches(string.Empty);
    }

    [Fact]
    public void EmptyStringText_RendersNothing()
    {
        var piece = new DiffPiece(string.Empty, ChangeType.Unchanged);

        var cut = RenderComponent<TextDiffLine>(parameters => parameters
            .Add(p => p.Model, piece));

        cut.MarkupMatches(string.Empty);
    }
}
