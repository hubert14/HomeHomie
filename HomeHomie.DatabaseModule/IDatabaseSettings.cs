namespace HomeHomie.DatabaseModule
{
    public interface IDatabaseSettings
    {
        public string? ConnectionString { get; }
        public string? DatabaseName { get; }
    }
}
