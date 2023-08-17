using HomeHomie.Core.DI;
using HomeHomie.Core.Extensions;
using HomeHomie.ElectricityModule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Home Homie. ElecticityModule. Start working");

static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration(config => config.AddAppSettings())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddCoreDependencies(hostContext.Configuration);
                services.ConfigureSettings<IElectricitySettings, ElectricitySettings>(hostContext.Configuration);

                services.AddHostedService<ReportRecieverService>();
                services.AddSingleton<ReportSaver>();
            });

var host = CreateHostBuilder(args).Build();
host.Services.GetRequiredService<ReportSaver>(); // Get service here to start recieving messages from broker
await host.RunAsync();
