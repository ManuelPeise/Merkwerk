namespace Data.IntegrationTests.Infrastructure;

/// <summary>Same MySQL version as production (ADR 004, deploy/docker-compose.yml).</summary>
public static class MySqlImage
{
    public const string Name = "mysql:8.4";
}
