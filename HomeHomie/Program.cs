using HomeHomie;
using HomeHomie.Background;
using HomeHomie.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Home Homie Start working");

static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder().ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<TelegramClient>();
                services.AddHostedService<TelegramNotifierWorker>();
                services.AddHostedService<ReportRecieverWorker>();

            });

var host = CreateHostBuilder(args).Build();
host.Services.GetRequiredService<TelegramClient>();
await host.RunAsync();
