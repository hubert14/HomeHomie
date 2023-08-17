using HomeHomie.Core.DI;
using HomeHomie.Core.Extensions;
using HomeHomie.TelegramModule.Settings;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Home Homie. Home Assistant Module. Start working");

static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddAppSettings())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddCoreDependencies(hostContext.Configuration);
                services.ConfigureSettings<IHomeAssistantSettings, HomeAssistantSettings>(hostContext.Configuration);
            });

var host = CreateHostBuilder(args).Build();
await host.RunAsync();
