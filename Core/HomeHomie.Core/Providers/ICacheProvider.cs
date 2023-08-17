namespace HomeHomie.Core.Providers
{
    public interface ICacheProvider
    {
        public Task<T?> GetAsync<T>(string key, bool removeAfterGet = true);
        public Task<bool> SetAsync<T>(string key, T value, TimeSpan? lifeTime = null);
        public Task<bool> RemoveAsync(string key);
    }
}
