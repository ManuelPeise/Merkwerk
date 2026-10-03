using Web.Core.Bundels;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.ConfigureServices(builder.Configuration);

var app = builder.AddAppConfiguration();

app.Run();

/// <summary>Entry point, public for WebApplicationFactory in integration tests.</summary>
public partial class Program;
