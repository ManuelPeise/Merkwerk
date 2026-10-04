using Web.Core.Bundles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.AddAppConfiguration();

// Pending migrations are applied before the first request (ADR 016).
await app.MigrateDatabaseAsync();

await app.RunAsync();

/// <summary>Entry point, public for WebApplicationFactory in integration tests.</summary>
public partial class Program;
