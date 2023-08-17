using HomeHomie.Core.Extensions;
using HomeHomie.Core.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.Core.MessageBroker
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddMessageBroker(this IServiceCollection serviceCollection) => serviceCollection
            .AddSingleton<IBrokerProvider, RabbitMQBroker>();

        public static IServiceCollection AddMessageBrokerSettings(this IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection
            .ConfigureSettings<IBrokerSettings, BrokerSettings>(configuration, "Broker");
    }
}
