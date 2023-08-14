using HomeHomie.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Home Homie Start working");

static IHostBuilder CreateHostBuilder(string[] args) => new HostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.AddJsonFile("appsettings.json");
                config.AddJsonFile("appsettings.local.json", true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSettings(hostContext.Configuration);
                services.AddDependencies();
            });

var host = CreateHostBuilder(args).Build();
//var telegramService = host.Services.GetRequiredService<TelegramClient>();

//var notifySetting = await MongoProvider.GetSettingsFromMongoAsync<NotifyOnBootSettings>(Variables.NOTIFY_ON_BOOT);
//if(notifySetting.Notify)
//{
//    var templateSetting = await MongoProvider.GetSettingsFromMongoAsync<TelegramTemplatesSettings>(Variables.TELEGRAM_TEMPLATES);
//    var notifyTemplate = templateSetting.Templates.Single(x => x.Title == TelegramTemplates.BOOT);
//    await TelegramProvider.SendMessagesToTelegramChatsAsync(notifyTemplate.Template);
//}
//telegramService.StartReceiving();

await host.RunAsync();
