using BlazorTextDiff.Web;
using DiffPlex.DiffBuilder;
using DiffPlex;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure root components with modern selectors
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure services
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Register DiffPlex services
builder.Services.AddScoped<ISideBySideDiffBuilder, SideBySideDiffBuilder>();
builder.Services.AddScoped<IDiffer, Differ>();

// Enable logging for development
if (builder.HostEnvironment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

await builder.Build().RunAsync();
