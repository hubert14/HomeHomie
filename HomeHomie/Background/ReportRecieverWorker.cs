using HomeHomie.Database;
using HomeHomie.Database.Entities;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace HomeHomie.Background
{
    internal class ReportRecieverWorker : IHostedService
    {
        private Timer? _timer = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(CheckGraphics, null, TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private async void CheckGraphics(object? state)
        {
            var date = DateTime.Now.ToString("dd.MM.yyyy");
            var graphic = await MongoProvider.GetDataFromMongoAsync(date);

            if (graphic != null)
            {
                return;
            }
            else
            {
                Console.WriteLine($"Try to pull graphic from website for date: " + date);
                Stream imageStream;
                try
                {
                    imageStream = await PullGraphicAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

                var data = ProcessImage(imageStream);
                var newGraphic = new ElectricityGraphic
                {
                    Date = date,
                    OnHours = data.Where(x => x.IsLightOn).Select(x => x.Hour).ToList()
                };

                await MongoProvider.AddDataInMongoAsync(newGraphic);
            }
        }

        private async Task<Stream> PullGraphicAsync()
        {
            var httpClient = new HttpClient();
            var siteUrl = "https://" + Environment.GetEnvironmentVariable(Variables.GRAPHIC_DOMAIN);
            var html = await httpClient.GetStringAsync(siteUrl + "/" + Environment.GetEnvironmentVariable(Variables.GRAPHIC_PAGE));

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var values = document.DocumentNode
                .SelectNodes("//img")
                .FirstOrDefault(x => x.GetAttributeValue("src", "~unknown").Contains("current-timetable"));

            var url = values.GetAttributeValue("src", "~unknown");

            var imageName = url.Split('/').Last();
            var nextDay = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            if (!imageName.Contains(nextDay)) throw new Exception("This graphic not for tomorrow. Graphic name: " + imageName);

            return await httpClient.GetStreamAsync(siteUrl + url);
        }

        private List<(int Hour, bool IsLightOn)> ProcessImage(Stream imageStream)
        {
            using var img = Image.Load<Rgb24>(imageStream);

            var heigth = img.Size().Height;
            var startHeightPixel = heigth - 8;
            var startWidthPixel = 40;

            var black = new Rgb24(0, 0, 0);
            var blue = new Rgb24(0, 176, 240);
            var white = new Rgb24(255, 255, 255);
            var gray = new Rgb24(191, 191, 191);

            var startTrigger = true;
            var grayTrigger = false;
            var blackTrigger = false;

            var lastPixel = blue;
            var currentHour = 0;

            List<bool> values = Enumerable.Repeat(false, 24).ToList();

            for (int i = startWidthPixel; i < img.Size().Width; i++)
            {
                var currentPixel = img[i, startHeightPixel];
                if (currentPixel == lastPixel) continue;
                lastPixel = currentPixel;

                if (startTrigger && currentPixel == blue) continue;

                if (startTrigger && currentPixel == black)
                {
                    startTrigger = false;
                    blackTrigger = true;
                    continue;
                }

                if (currentPixel == gray && blackTrigger)
                {
                    grayTrigger = true;
                    blackTrigger = false;
                    continue;
                }

                if (currentPixel == black && grayTrigger)
                {
                    grayTrigger = false;
                    blackTrigger = true;
                    continue;
                }

                if (currentPixel == blue && blackTrigger)
                {
                    blackTrigger = false;
                    values[currentHour] = false;
                    continue;
                }

                if (currentPixel == white && blackTrigger)
                {
                    blackTrigger = false;
                    values[currentHour] = true;
                    continue;
                }

                if (currentPixel == black)
                {
                    blackTrigger = true;
                    currentHour++;
                    if (currentHour == 24) break;
                    continue;
                }
            }

            var counter = 0;
            return values.Select(isLightOn => (counter++, isLightOn)).ToList();
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
