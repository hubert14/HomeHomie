namespace HomeHomie.Core.Providers
{
    public interface ICacheProvider
    {
        public Task<string> GetAsync(string key, bool removeAfterGet = true);
        public Task<bool> SetAsync(string key, string value, TimeSpan? lifeTime = null);
        public Task<bool> RemoveAsync(string key);
    }
}
