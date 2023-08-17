namespace HomeHomie.Core.Database
{
    public interface IDatabaseSettings
    {
        public string? ConnectionString { get; }
        public string? DatabaseName { get; }
    }
}
