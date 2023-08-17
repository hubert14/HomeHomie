namespace HomeHomie.Core.Database
{
    internal class DatabaseSettings : IDatabaseSettings
    {
        public string? ConnectionString { get; init; }

        public string? DatabaseName { get; init; }
    }
}
