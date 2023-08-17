using HomeHomie.Core.Extensions;
using HomeHomie.Core.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.Core.Cache
{
    public static class DI
    {
        public static IServiceCollection AddCache(this IServiceCollection services) => services
            .AddSingleton<ICacheProvider, RedisCacheProvider>();

        public static IServiceCollection AddCacheSettings(this IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection
            .ConfigureSettings<ICacheSettings, CacheSettings>(configuration, "Cache");
    }
}
