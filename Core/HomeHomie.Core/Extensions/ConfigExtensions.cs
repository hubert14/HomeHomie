using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HomeHomie.Core.Extensions
{
    public static class ConfigExtensions
    {
        public static IConfigurationBuilder AddAppSettings(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddJsonFile("appsettings.core.json")
                .AddJsonFile("appsettings.core.local.json", optional: true)
                .AddEnvironmentVariables();
        }

        public static IServiceCollection ConfigureSettings<TInterface, TImplementation>(this IServiceCollection services, IConfiguration config, string? section = null)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            if (!string.IsNullOrWhiteSpace(section)) services.Configure<TImplementation>(c => config.GetSection(section).Bind(c));
            else services.Configure<TImplementation>(c => config.Bind(c));

            services.AddSingleton<TInterface>(sp => sp.GetRequiredService<IOptions<TImplementation>>().Value);

            return services;
        }
    }
}
