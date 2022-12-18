using HomeHomie.Database;
using HomeHomie.Database.Entities;
using HomeHomie.Telegram;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static HomeHomie.Utils;

namespace HomeHomie.Background
{
    internal class ReportRecieverWorker : IHostedService
    {
        private Timer? _timer = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ReceiveReports, null, TimeSpan.FromMinutes(0), TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private async void ReceiveReports(object? state)
        {
            await CheckGraphics(DateTime.Now.AddDays(-1));
            await CheckGraphics(DateTime.Now);
            await CheckGraphics(DateTime.Now.AddDays(+1));
        }

        private async Task CheckGraphics(DateTime date)
        {
            var stringDate = date.ToString("dd.MM.yyyy");
            var graphic = await MongoProvider.GetDataFromMongoAsync(stringDate);

            if (graphic != null) return;
            else
            {
                Console.WriteLine($"Try to pull graphic from website for date: " + stringDate);
                string imageLink;
                Stream imageStream;
                try
                {
                    (imageLink, imageStream) = await PullGraphicAsync(date);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

                var data = ProcessImage(imageStream);
                var newGraphic = new ElectricityGraphic
                {
                    Date = stringDate,
                    ImageLink = imageLink,
                    OnHours = data.Where(x => x.IsLightOn).Select(x => x.Hour).ToList()
                };

                await MongoProvider.AddDataInMongoAsync(newGraphic);
                await TelegramProvider.SendMediaMessageAsync(imageLink, newGraphic.ToString());
            }
        }

        private async Task<(string imageLink, Stream imageStream)> PullGraphicAsync(DateTime date)
        {
            var httpClient = new HttpClient();
            var siteUrl = "https://" + GetVariable(Variables.GRAPHIC_DOMAIN);
            var html = await httpClient.GetStringAsync(siteUrl + "/" + GetVariable(Variables.GRAPHIC_PAGE));

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var values = document.DocumentNode
                .SelectNodes("//img")
                .FirstOrDefault(x => x.GetAttributeValue("src", "~unknown").Contains("current-timetable"));

            var url = values.GetAttributeValue("src", "~unknown");

            var imageName = url.Split('/').Last();
            var nextDay = date.AddDays(1).ToString("yyyyMMdd");
            var twoDays = date.Day + "_" + date.AddDays(1).ToString("ddMMyy");

            if (!imageName.Contains(nextDay) && !imageName.Contains(twoDays)) throw new Exception("This graphic not for requested date. Graphic name: " + imageName + " | Date: " + date.ToString("dd.MM.yy"));

            return (siteUrl + url, await httpClient.GetStreamAsync(siteUrl + url));
        }

        private List<(int Hour, bool IsLightOn)> ProcessImage(Stream imageStream)
        {
            using var img = Image.Load<Rgb24>(imageStream);

            var heigth = img.Size().Height;
            var startHeightPixel = heigth - 15;
            var startWidthPixel = 155;

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
