namespace Logic.Devices.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class DevicesDatabaseCollection : ICollectionFixture<DevicesDatabaseFixture>
{
    public const string Name = "devices-database";
}
