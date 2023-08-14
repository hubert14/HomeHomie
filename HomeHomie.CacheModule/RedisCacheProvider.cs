using HomeHomie.Core.Providers;
using StackExchange.Redis;
using System.Text.Json;

namespace HomeHomie.CacheModule
{
    internal class RedisCacheProvider : ICacheProvider
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisCacheProvider(ICacheSettings cacheSettings)
        {
            _redis = ConnectionMultiplexer.Connect(cacheSettings.Address ?? throw new ArgumentNullException($"Cache {nameof(cacheSettings.Address)} is not defined under Cache:Address setting"));
        }

        public async Task<T?> GetAsync<T>(string key, bool removeAfterGet = true)
        {
            var str = await (removeAfterGet
                ? _redis.GetDatabase().StringGetDeleteAsync(key)
                : _redis.GetDatabase().StringGetAsync(key));

            return JsonSerializer.Deserialize<T>(str.ToString());
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? lifeTime = null)
        {
            var str = JsonSerializer.Serialize<T>(value);

            return await _redis.GetDatabase().StringSetAsync(key, str, lifeTime);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _redis.GetDatabase().KeyDeleteAsync(key);
        }
    }
}
