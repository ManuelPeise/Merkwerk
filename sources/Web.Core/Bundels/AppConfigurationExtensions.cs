namespace Web.Core.Bundels;

/// <summary>Builds the app and configures the HTTP request pipeline, called once from Program.cs.</summary>
public static class AppConfigurationExtensions
{
    public static WebApplication AddAppConfiguration(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            // OpenAPI document (/openapi/v1.json) and Swagger UI (/swagger) – development only.
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Merkwerk API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "Merkwerk API";
            });
        }
        else
        {
            // Unhandled exceptions become ProblemDetails without internal details.
            app.UseExceptionHandler();
            app.UseHsts();
        }

        app.UseCors();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
