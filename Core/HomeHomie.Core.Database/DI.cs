using HomeHomie.Core.Extensions;
using HomeHomie.Core.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.Core.Database
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection serviceCollection) => serviceCollection
            .AddSingleton<IDatabaseProvider, MongoProvider>();

        public static IServiceCollection AddDatabaseSettings(this IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection
            .ConfigureSettings<IDatabaseSettings, DatabaseSettings>(configuration, "Database");
    }
}
