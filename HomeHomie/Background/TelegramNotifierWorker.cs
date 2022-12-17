using HomeHomie.Database;
using HomeHomie.Telegram;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace HomeHomie.Background
{
    internal class TelegramNotifierWorker : IHostedService
    {
        private Timer? _timer = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckGraphics, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }

        private async void CheckGraphics(object? state)
        {
            var date = DateTime.Now.ToString("dd.MM.yyyy");

            var graphic = await MongoProvider.GetDataFromMongoAsync(date);

            if (graphic != null)
            {
                Console.WriteLine("Found graphic for " + graphic.Date);

                var currentHour = DateTime.Now.Hour;

                if (graphic.OffHours.Contains(currentHour + 1) && !graphic.NotifiedHours.Contains(currentHour + 1))
                {
                    var nearestOnHour = graphic.OnHours.FirstOrDefault(x => x > currentHour + 1);

                    await TelegramProvider.SendMessagesToTelegramChatsAsync(currentHour + 1, nearestOnHour);
                    for (int i = currentHour + 1; i < nearestOnHour; i++)
                    {
                        graphic.NotifiedHours.Add(i);
                    }
                    await MongoProvider.UpdateDataInMongoAsync(graphic);
                }
            }
            else
            {
                Console.WriteLine("Graphic for " + date + " was not found");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
