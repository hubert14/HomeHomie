using HomeHomie.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace HomeHomie.DatabaseModule
{
    public static class ServiceProviderExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDatabaseProvider, MongoProvider>();
            return serviceCollection;
        }
    }
}
