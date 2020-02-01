# Blazor Text Diff
A component to display side by side text diff using the [DiffPlex](https://github.com/mmanela/diffplex) library. There is probably some issues that I have looked over so if you notice one please submit an issue or do a pull request!


[![publish to nuget](https://github.com/lzinga/BlazorTextDiff/workflows/publish%20to%20nuget/badge.svg)![Nuget](https://img.shields.io/nuget/v/BlazorTextDiff)](https://www.nuget.org/packages/BlazorTextDiff)


![Static Diff](https://i.imgur.com/t0nJPeZ.png)
![Async Diff](https://i.imgur.com/lzjfjhF.png)

# Installation
You will need to add the nuget package DiffPlex into your project for this to work. An example project can be found in the [Samples Folder](https://github.com/lzinga/BlazorTextDiff/tree/master/samples/BlazorTextDiff.Web) for implementation.

```csharp
// Program.cs
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    
    // These must be injected into your application to supply the component with its diff checking.
    builder.Services.AddScoped<ISideBySideDiffBuilder, SideBySideDiffBuilder>();
    builder.Services.AddScoped<IDiffer, Differ>();

    builder.RootComponents.Add<App>("app");

    await builder.Build().RunAsync();
}
```

```html
<!-- Index.html -->
<link href="_content/BlazorTextDiff/css/BlazorDiff.css" rel="stylesheet" />
<script src="_content/BlazorTextDiff/js/BlazorTextDiff.js"></script>
```


# Usage
```html
<TextDiff

  <!-- The left side of the comparison -->
  OldText="Old Text"
  
  <!-- The right side of the comparison -->
  NewText="New Text"

  <!-- Determines if the div containing the diffs should be collapsed if there is a lot of data. -->
  <!-- There is currently an issue where the toggling doesn't work accurately with the js interop. -->
  CollapseContent="true"
  
  <!-- Converts space values into \u00B7 and tab values into \u00B7\u00B7 -->
  ShowWhiteSpace="true"

>
    <Header>
      <!-- Context Variables -->
      <!-- @context.Additions -->
      <!-- @context.Modifications -->
      <!-- @context.Deletions -->
    </Header>
</TextDiff>
```
