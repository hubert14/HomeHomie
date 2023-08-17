using HomeHomie.Core.DI;
using HomeHomie.Core.Extensions;
using HomeHomie.Core.Providers;
using HomeHomie.TelegramModule;
using HomeHomie.TelegramModule.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Home Homie. Telegram Module. Start working");

static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddAppSettings())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddCoreDependencies(hostContext.Configuration);
                services.ConfigureSettings<ITelegramSettings, TelegramSettings>(hostContext.Configuration);

                services.AddSingleton<INotificationProducerProvider, TelegramSender>();
                services.AddSingleton<INotificationConsumerProvider, TelegramReciever>();
            });

var host = CreateHostBuilder(args).Build();
host.Services.GetRequiredService<INotificationProducerProvider>().StartProducing();
await host.RunAsync();
