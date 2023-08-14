using HomeHomie.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.MessageBrokerModule
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddMessageBroker(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IBrokerProvider, RabbitMQBroker>();
            return serviceCollection;
        }
    }
}
