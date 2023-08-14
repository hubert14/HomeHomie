using HomeHomie.CacheModule;
using HomeHomie.DatabaseModule;
using HomeHomie.ElectricityModule;
using HomeHomie.MessageBrokerModule;
using HomeHomie.TelegramModule;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.Infrastructure
{
    public static class ServiceProviderExtensions
    {
        public static void AddDependencies(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddCache()
                .AddDatabase()
                .AddMessageBroker();
        }

        public static void AddSettings(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IBrokerSettings>(new BrokerSettings(configuration));
            serviceCollection.AddSingleton<ICacheSettings>(new CacheSettings(configuration));
            serviceCollection.AddSingleton<IDatabaseSettings>(new DatabaseSettings(configuration));
            serviceCollection.AddSingleton<IElectricitySettings>(new ElectricitySettings(configuration));
            serviceCollection.AddSingleton<ITelegramSettings>(new TelegramSettings(configuration));
        }
    }
}
