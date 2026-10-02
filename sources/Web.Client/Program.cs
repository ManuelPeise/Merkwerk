using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web.Client.Services;
using Web.Client.Services.DragDrop;
using Web.Client.Services.Speech;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// API access with the JWT cookie; on 401 the handler refreshes the token once and retries (LP-006).
var refreshingHandler = new RefreshingHandler { InnerHandler = new HttpClientHandler() };
builder.Services.AddSingleton(refreshingHandler);
builder.Services.AddScoped(_ => new HttpClient(refreshingHandler, disposeHandler: false)
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

// JS interop wrappers (LP-007): SortableJS and Web Speech API.
builder.Services.AddScoped<DragDropService>();
builder.Services.AddScoped<SpeechService>();

await builder.Build().RunAsync();
