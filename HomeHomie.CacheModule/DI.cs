using HomeHomie.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.CacheModule
{
    public static class DI
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            return services.AddSingleton<ICacheProvider, RedisCacheProvider>();
        }
    }
}
