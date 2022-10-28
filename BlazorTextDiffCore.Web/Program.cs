using BlazorTextDiffCore.Web;
using DiffPlex.DiffBuilder;
using DiffPlex;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ISideBySideDiffBuilder, SideBySideDiffBuilder>();
builder.Services.AddScoped<IDiffer, Differ>();

await builder.Build().RunAsync();
