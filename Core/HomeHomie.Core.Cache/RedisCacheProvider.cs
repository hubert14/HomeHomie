using HomeHomie.Core.Providers;
using StackExchange.Redis;
using System.Text.Json;

namespace HomeHomie.Core.Cache
{
    internal class RedisCacheProvider : ICacheProvider
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisCacheProvider(ICacheSettings cacheSettings)
        {
            _redis = ConnectionMultiplexer.Connect(cacheSettings.Address ?? throw new ArgumentNullException($"Cache {nameof(cacheSettings.Address)} is not defined under Cache:Address setting"));
        }

        public async Task<string> GetAsync(string key, bool removeAfterGet = true)
        {
            var str = await (removeAfterGet
                ? _redis.GetDatabase().StringGetDeleteAsync(key)
                : _redis.GetDatabase().StringGetAsync(key));

            if (string.IsNullOrWhiteSpace(str)) return null;

            return str.ToString();
        }

        public async Task<bool> SetAsync(string key, string value, TimeSpan? lifeTime = null)
        {
            return await _redis.GetDatabase().StringSetAsync(key, value, lifeTime);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _redis.GetDatabase().KeyDeleteAsync(key);
        }
    }
}
