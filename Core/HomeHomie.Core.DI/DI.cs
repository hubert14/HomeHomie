using HomeHomie.Core.Cache;
using HomeHomie.Core.Database;
using HomeHomie.Core.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.Core.DI
{
    public static class DependenciesExtensions
    {
        public static IServiceCollection AddCoreDependencies(this IServiceCollection services, IConfiguration configuration) => services
            .AddDependencies()
            .AddSettings(configuration);

        public static IServiceCollection AddDependencies(this IServiceCollection serviceCollection) => serviceCollection
            .AddCache()
            .AddDatabase()
            .AddMessageBroker();

        public static IServiceCollection AddSettings(this IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection
            .AddCacheSettings(configuration)
            .AddDatabaseSettings(configuration)
            .AddMessageBrokerSettings(configuration);
    }
}
