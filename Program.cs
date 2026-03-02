using GPSDataRenderer;
using GPSDataRenderer.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Root component
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient for fetching JSON from wwwroot/static-data
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register your services for DI
builder.Services.AddScoped<IJsonLoader, JsonLoader>();
builder.Services.AddScoped<IDataService, DataService>();

await builder.Build().RunAsync();