# BlazorTextDiff 🔍

A modern Blazor component for displaying side-by-side text differences with syntax highlighting and advanced comparison features. Built on top of the powerful [DiffPlex](https://github.com/mmanela/diffplex) library.

## 🚀 Features

- **Side-by-side comparison** with clear visual indicators
- **Syntax highlighting** for better readability
- **Ignore case and whitespace** options
- **Async diff processing** for large texts
- **Customizable headers** with diff statistics
- **Responsive design** that works on all devices
- **Easy integration** with existing Blazor applications

## 📊 Status

[![Build and Publish Packages](https://github.com/lzinga/BlazorTextDiff/actions/workflows/publish-packages.yml/badge.svg)](https://github.com/lzinga/BlazorTextDiff/actions/workflows/publish-packages.yml)
[![Deploy to GitHub Pages](https://github.com/lzinga/BlazorTextDiff/actions/workflows/deploy-pages.yml/badge.svg)](https://github.com/lzinga/BlazorTextDiff/actions/workflows/deploy-pages.yml)
[![NuGet](https://img.shields.io/nuget/v/BlazorTextDiff.svg)](https://www.nuget.org/packages/BlazorTextDiff/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/BlazorTextDiff.svg)](https://www.nuget.org/packages/BlazorTextDiff/)

## 🎮 Live Demo

Try the interactive demo: [https://lzinga.github.io/BlazorTextDiff/](https://lzinga.github.io/BlazorTextDiff/)

## 📸 Screenshots

![Static Diff](https://i.imgur.com/t0nJPeZ.png)
*Basic text comparison showing additions, deletions, and modifications*

![Async Diff](https://i.imgur.com/lzjfjhF.png)
*Async processing for large text comparisons*

## 📦 Installation

Install the NuGet package:

```bash
dotnet add package BlazorTextDiff
```

You'll also need the DiffPlex library:

```bash
dotnet add package DiffPlex
```

## ⚙️ Setup

### 1. Configure Services

Add the required services to your `Program.cs`:

```csharp
// Program.cs
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    
    // Register BlazorTextDiff dependencies
    builder.Services.AddScoped<ISideBySideDiffBuilder, SideBySideDiffBuilder>();
    builder.Services.AddScoped<IDiffer, Differ>();

    builder.RootComponents.Add<App>("app");

    await builder.Build().RunAsync();
}
```

### 2. Include Styles and Scripts

Add to your `index.html` or `_Host.cshtml`:

```html
<!-- Required CSS -->
<link href="_content/BlazorTextDiff/css/BlazorDiff.css" rel="stylesheet" />

<!-- Required JavaScript -->
<script src="_content/BlazorTextDiff/js/BlazorTextDiff.js"></script>
```

## 🎯 Usage

### Basic Comparison

```html
<TextDiff OldText="@oldText" NewText="@newText" />
```

### Advanced Features

```html
<TextDiff
    OldText="@oldText"
    NewText="@newText"
    CollapseContent="true"
    ShowWhiteSpace="true"
    IgnoreCase="true"
    IgnoreWhiteSpace="false">
    
    <Header>
        <div class="diff-stats">
            <span class="badge bg-success">+@context.Additions</span>
            <span class="badge bg-warning">~@context.Modifications</span>
            <span class="badge bg-danger">-@context.Deletions</span>
        </div>
    </Header>
</TextDiff>
```

### Async Processing

For large texts, use async processing:

```csharp
@code {
    private string oldText = "";
    private string newText = "";
    private bool isProcessing = false;

    private async Task ProcessLargeDiff()
    {
        isProcessing = true;
        // Your async logic here
        await Task.Delay(100); // Simulate processing
        isProcessing = false;
    }
}
```

## 🔧 Component Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `OldText` | `string` | `""` | The original text (left side) |
| `NewText` | `string` | `""` | The modified text (right side) |
| `CollapseContent` | `bool` | `false` | Collapse large diff sections |
| `ShowWhiteSpace` | `bool` | `false` | Visualize spaces and tabs |
| `IgnoreCase` | `bool` | `false` | Ignore case differences |
| `IgnoreWhiteSpace` | `bool` | `false` | Ignore whitespace differences |

## 🎨 Customization

The component uses CSS classes that you can override:

```css
.diff-container { /* Main container */ }
.diff-line-added { /* Added lines */ }
.diff-line-deleted { /* Deleted lines */ }
.diff-line-modified { /* Modified lines */ }
.diff-line-unchanged { /* Unchanged lines */ }
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [DiffPlex](https://github.com/mmanela/diffplex) - The core diffing library
- [Blazor](https://blazor.net/) - The web framework that makes this possible

---

<div align="center">
  Made with ❤️ for the Blazor community
</div>
