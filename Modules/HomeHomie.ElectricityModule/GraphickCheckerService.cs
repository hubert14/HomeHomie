using HomeHomie.Core.Providers;
using HomeHomie.ElectricityModule.Models;
using Microsoft.Extensions.Hosting;
using static HomeHomie.Core.Models.Messages.TelegramMessages;

namespace HomeHomie.ElectricityModule
{
    internal class GraphickCheckerService : BackgroundService
    {
        private readonly IDatabaseProvider _database;
        private readonly IBrokerProvider _broker;
        private readonly IElectricitySettings _settings;

        public GraphickCheckerService(IDatabaseProvider database, IBrokerProvider broker, IElectricitySettings settings)
        {
            _database = database;
            _broker = broker;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckGraphics();

                try
                {
                    await Task.Delay(TimeSpan.Parse(_settings.GraphicCheckDelay!), stoppingToken);
                }
                catch (TaskCanceledException exception)
                {
                    Console.WriteLine("Task was canceled. {0}", exception.Message);
                }
            }
        }

        private async Task CheckGraphics()
        {
            var date = DateTime.Now.ToString("dd.MM.yyyy");

            var graphic = _database.Get<ElectricityGraphic>().FirstOrDefault(x => x.Date == date);

            if (graphic != null)
            {
                Console.WriteLine("Found graphic for " + graphic.Date);

                var currentHour = DateTime.Now.Hour;

                if (graphic.OffHours.Contains(currentHour + 1) && !(graphic.NotifiedHours?.Contains(currentHour + 1) ?? false))
                {
                    var nearestOnHour = graphic.OnHours.FirstOrDefault(x => x > currentHour + 1);

                    const string template = "😵 Совсем скоро выключат свет!\r\nВремя отключения: {offTime}:00\r\nСледующее включение: {onTime}:00";

                    var text = template
                        .Replace("{offTime}", (currentHour + 1).ToString())
                        .Replace("{onTime}", nearestOnHour.ToString());

                    _broker.SendMessage(new SendTelegramMessageRequest { Message = text });

                    graphic.NotifiedHours ??= new();
                    graphic.NotifiedHours.Add(currentHour + 1);
                    for (int i = currentHour + 2; i < 24; i++)
                    {
                        if (graphic.OnHours.Contains(i)) break;
                        graphic.NotifiedHours.Add(i);
                    }

                    await _database.ReplaceAsync(graphic);
                }
            }
            else Console.WriteLine("Graphic for " + date + " was not found");
        }
    }
}
