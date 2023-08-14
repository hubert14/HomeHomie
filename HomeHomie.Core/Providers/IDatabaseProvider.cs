namespace HomeHomie.Core.Providers
{
    public interface IDatabaseProvider
    {
        public IQueryable<T> Get<T>() where T: BaseEntity;
        public Task<T> GetAsync<T>(Guid id) where T : BaseEntity;
        public Task InsertAsync<T>(T item) where T : BaseEntity;
        public Task ReplaceAsync<T>(T item) where T : BaseEntity;
        public Task DeleteAsync<T>(Guid id) where T : BaseEntity;
    }
}