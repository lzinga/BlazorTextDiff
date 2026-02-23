# BlazorTextDiff

A Blazor component for displaying side-by-side text differences with character-level highlighting. Built on [DiffPlex](https://github.com/mmanela/diffplex).

## Features

- Side-by-side diff with line numbers
- Character-level highlighting within changed lines
- Word-level soft highlight with character-level strong highlight for partial changes
- Adjacent character highlights merge into smooth pill shapes
- Collapse/expand unchanged sections
- Ignore case and whitespace options
- Custom header with diff statistics
- Custom CSS class and attribute support
- Fully themeable via CSS custom properties
- Dark mode via `prefers-color-scheme`
- Responsive and accessible

## Status

[![Build and Publish Packages](https://github.com/lzinga/BlazorTextDiff/actions/workflows/publish-packages.yml/badge.svg)](https://github.com/lzinga/BlazorTextDiff/actions/workflows/publish-packages.yml)
[![Deploy to GitHub Pages](https://github.com/lzinga/BlazorTextDiff/actions/workflows/deploy-pages.yml/badge.svg)](https://github.com/lzinga/BlazorTextDiff/actions/workflows/deploy-pages.yml)
[![NuGet](https://img.shields.io/nuget/v/BlazorTextDiff.svg)](https://www.nuget.org/packages/BlazorTextDiff/)

## Live Demo

[https://lzinga.github.io/BlazorTextDiff/](https://lzinga.github.io/BlazorTextDiff/)

## Installation

```bash
dotnet add package BlazorTextDiff
```

## Setup

Add the stylesheet to your `index.html` or `_Host.cshtml`:

```html
<link href="_content/BlazorTextDiff/css/BlazorDiff.css" rel="stylesheet" />
```

No JavaScript or service registration is required.

## Usage

### Basic

```razor
<TextDiff OldText="@oldText" NewText="@newText" />
```

### With Options

```razor
<TextDiff OldText="@oldText"
          NewText="@newText"
          CollapseContent="true"
          IgnoreCase="true"
          IgnoreWhiteSpace="false"
          Class="my-diff">
    <Header>
        <div style="padding: 10px 12px;">
            <span class="diff-stats-badge warning">@context.LineModificationCount modified</span>
            <span class="diff-stats-badge danger">@context.LineDeletionCount deleted</span>
            <span class="diff-stats-badge success">@context.LineAdditionCount added</span>
        </div>
    </Header>
</TextDiff>
```

## Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `OldText` | `string?` | `null` | Original text (left pane) |
| `NewText` | `string?` | `null` | Modified text (right pane) |
| `CollapseContent` | `bool` | `false` | Collapse unchanged sections |
| `MaxHeight` | `int` | `300` | Max height (px) when collapsed |
| `IgnoreCase` | `bool` | `false` | Ignore case differences |
| `IgnoreWhiteSpace` | `bool` | `false` | Ignore whitespace differences |
| `Header` | `RenderFragment<DiffStats>?` | `null` | Custom header template |
| `Class` | `string?` | `null` | Additional CSS class(es) |

Unmatched HTML attributes (`style`, `id`, `data-*`, etc.) are passed through to the root element.

## How Character Highlighting Works

The component uses three levels of visual hierarchy:

1. **Line-level** — the entire row gets a colored background (`inserted-line`, `deleted-line`, `modified-line`)
2. **Word-level** — when a word is partially changed, it gets a soft background highlight (`inserted-word`, `deleted-word`, `modified-word`)
3. **Character-level** — the specific changed characters get a strong highlight (`inserted-character`, `deleted-character`, `modified-character`)

For example, `Programing` → `Programming`:
- The whole word wraps in `<span class="inserted-word">` (soft green background)
- Only the added `m` wraps in `<span class="inserted-character">` (strong green highlight)

When a word is entirely changed (e.g. `cat` → `dog`), it skips the word wrapper and uses the character-level class directly.

Adjacent character highlights automatically merge into a single pill shape — rounded corners only appear on the first and last character in a run.

## Customization

All visual styling is controlled via CSS custom properties. Override them in your own stylesheet to retheme the component.

### Diff Colors

```css
:root {
  /* Line-level backgrounds */
  --diff-addition-bg: #e6ffed;
  --diff-deletion-bg: #ffeef0;
  --diff-modification-bg: #fff8c5;

  /* Line-level left border accents */
  --diff-addition-border: #2ea043;
  --diff-deletion-border: #f85149;
  --diff-modification-border: #fb8500;

  /* Character-level strong highlights */
  --diff-addition-highlight: #7ce89b;
  --diff-deletion-highlight: #f9a8b0;
  --diff-modification-highlight: #ffc833;
}
```

The word-level soft background reuses `--diff-*-bg` (same as the line), and the character-level strong highlight uses `--diff-*-highlight`.

### Highlight Shape

```css
:root {
  /* Character highlight pill shape */
  --diff-char-radius: 3px;       /* border-radius for each character span */
  --diff-char-padding: 1px 2px;  /* padding inside each character span */

  /* Word highlight shape */
  --diff-word-radius: 3px;       /* border-radius for the word wrapper */
  --diff-word-padding: 1px 0;    /* padding inside the word wrapper */
}
```

Examples:

```css
/* Sharp rectangles instead of rounded pills */
:root { --diff-char-radius: 0; --diff-word-radius: 0; }

/* Larger, more prominent pills */
:root { --diff-char-radius: 6px; --diff-char-padding: 2px 4px; }

/* Underline style (no background, border-bottom instead) */
.my-diff .inserted-character { background: none; border-bottom: 2px solid #2ea043; }
.my-diff .deleted-character  { background: none; border-bottom: 2px solid #f85149; text-decoration: line-through; }
```

### Scoped Overrides

Use the `Class` parameter to scope styles to a specific instance:

```css
.my-diff .modified-line { background-color: #ffe0b2; }
.my-diff .deleted-character { background-color: #e53935; color: #fff; }
```

### CSS Classes Reference

**Layout:**
`diff-container`, `diff-pane-left`, `diff-pane-right`, `diff-header`, `diff-panes`, `diff-expand-notice`

**Line-level (on `<td>`):**
`inserted-line`, `deleted-line`, `modified-line`, `unchanged-line`

**Word-level (on `<span>` wrapping a partially changed word):**
`inserted-word`, `deleted-word`, `modified-word`

**Character-level (on `<span>` wrapping specific changed characters):**
`inserted-character`, `deleted-character`, `modified-character`

**Stats badges:**
`diff-stats-badge`, `primary`, `success`, `danger`, `warning`, `info`

### All CSS Custom Properties

| Property | Default | Description |
|---|---|---|
| `--diff-bg-primary` | `#ffffff` | Main background |
| `--diff-bg-secondary` | `#f6f8fa` | Header/footer background |
| `--diff-bg-tertiary` | `#f1f3f4` | Hover background |
| `--diff-border-primary` | `#e1e4e8` | Border color |
| `--diff-text-primary` | `#24292e` | Main text color |
| `--diff-text-muted` | `#656d76` | Line number color |
| `--diff-text-accent` | `#0969da` | Accent/focus color |
| `--diff-addition-bg` | `#e6ffed` | Added line & word background |
| `--diff-addition-border` | `#2ea043` | Added line left border |
| `--diff-addition-highlight` | `#7ce89b` | Added character highlight |
| `--diff-deletion-bg` | `#ffeef0` | Deleted line & word background |
| `--diff-deletion-border` | `#f85149` | Deleted line left border |
| `--diff-deletion-highlight` | `#f9a8b0` | Deleted character highlight |
| `--diff-modification-bg` | `#fff8c5` | Modified line & word background |
| `--diff-modification-border` | `#fb8500` | Modified line left border |
| `--diff-modification-highlight` | `#ffc833` | Modified character highlight |
| `--diff-char-radius` | `3px` | Character highlight border-radius |
| `--diff-char-padding` | `1px 2px` | Character highlight padding |
| `--diff-word-radius` | `3px` | Word highlight border-radius |
| `--diff-word-padding` | `1px 0` | Word highlight padding |
| `--diff-shadow` | `0 1px 3px ...` | Container shadow |
| `--diff-shadow-hover` | `0 2px 6px ...` | Container shadow on hover |

Dark mode overrides are built in via `@media (prefers-color-scheme: dark)`.

## AI-Assisted Development

This project uses AI as a development tool to help improve and maintain the library. AI assists with tasks such as code refactoring, writing tests, updating documentation, and implementing new features. All changes are generally reviewed by a human before being merged, but due to limited contributor availability and a lack of pull requests, AI is used to keep the project moving forward and ensure it stays up to date.

If you spot anything that looks off or have suggestions, contributions and issues are always welcome.

## License

MIT — see [LICENSE](LICENSE).

## Acknowledgments

- [DiffPlex](https://github.com/mmanela/diffplex) — core diffing engine
- [Blazor](https://blazor.net/) — web framework
