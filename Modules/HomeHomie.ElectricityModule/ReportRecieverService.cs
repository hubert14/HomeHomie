using HomeHomie.Core.Providers;
using HomeHomie.ElectricityModule.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp.PixelFormats;
using static HomeHomie.Core.Models.Messages.TelegramMessages;

namespace HomeHomie.ElectricityModule
{
    public class ReportRecieverService : BackgroundService
    {
        private IDatabaseProvider _database;
        private IBrokerProvider _broker;
        private IElectricitySettings _settings;

        static Rgb24 CheckColor = new(132, 191, 71);

        public ReportRecieverService(IDatabaseProvider database, IBrokerProvider broker, IElectricitySettings settings)
        {
            _database = database;
            _broker = broker;
            _settings = settings;

            if (_settings.ReportCheckDelay is null) throw new ArgumentNullException(nameof(_settings.ReportCheckDelay));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_settings.DatesToCheck is null) return;

            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessAsync();

                try
                {
                    await Task.Delay(TimeSpan.Parse(_settings.ReportCheckDelay!), stoppingToken);
                }
                catch (TaskCanceledException exception)
                {
                    Console.WriteLine("Task was canceled. {0}", exception.Message);
                }
            }
        }

        public async Task<bool> ProcessAsync()
        {
            if (_settings.DatesToCheck!.Count == 0) return true;

            foreach (var date in _settings.DatesToCheck.Select(x => DateTime.Now.AddDays(x)))
            {
                await CheckGraphics(date);
            }

            return true;
        }

        private async Task CheckGraphics(DateTime date)
        {
            var stringDate = date.ToString("dd.MM.yyyy");
            ElectricityGraphic graphic = _database.Get<ElectricityGraphic>().Where(x => x.Date == stringDate).FirstOrDefault();

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

                try
                {
                    var data = ProcessImage(imageStream);
                    var newGraphic = new ElectricityGraphic
                    {
                        Date = stringDate,
                        ImageLink = imageLink,
                        OnHours = data.Where(x => x.IsLightOn).Select(x => x.Hour).ToList()
                    };

                    await _database.InsertAsync(newGraphic);

                }
                catch (Exception e)
                {
                    await Console.Out.WriteLineAsync(e.Message);
                    _broker.SendMessage(new SendTelegramMessageRequest
                    {
                        Message = e.Message,
                        ReportDate = stringDate,
                        MediaLink = imageLink,
                        IsServiceMessage = true
                    });
                }
            }
        }

        private async Task<(string imageLink, Stream imageStream)> PullGraphicAsync(DateTime date)
        {
            var httpClient = new HttpClient();
            var siteUrl = "https://zakarpat.energy/customers/break-in-electricity-supply/schedule/";
            var html = await httpClient.GetStringAsync(siteUrl);

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var values = document.DocumentNode
                .SelectNodes("//img")
                .FirstOrDefault(x => x.GetAttributeValue("src", "~unknown").Contains("timetable-now"))
                ?? throw new Exception("Cannot found image in the graphic page");

            var url = values.GetAttributeValue("src", "~unknown");

            var imageName = url.Split('/').Last();
            var today = date.ToString("dd.MM.yy");
            var nextDay = date.AddDays(1).ToString("dd.MM.yy");
            var twoDays = date.Day + "_" + date.AddDays(1).ToString("dd.MM.yy");
            var previousAndToday = date.AddDays(-1).Day + "_" + date.AddDays(1).ToString("dd.MM.yy");

            if (!imageName.Contains(today) && !imageName.Contains(nextDay) && !imageName.Contains(twoDays))
            {
                throw new Exception(
                    "This graphic not for requested date. Graphic name: " + imageName +
                    " | Date: " + date.ToString("dd.MM.yy"));
            }

            return (_settings!.GraphicDomain + url, await httpClient.GetStreamAsync(_settings!.GraphicDomain + url));
        }

        private List<(int Hour, bool IsLightOn)> ProcessImage(Stream imageStream)
        {
            using var img = Image.Load<Rgb24>(imageStream);

            var heigth = img.Height;
            var startHeightPixel = FindHeightStartPixel(img);
            var startWidthPixel = FindWidthStartPixel(img, startHeightPixel);

            var black = new Rgb24(0, 0, 0);
            var white = new Rgb24(255, 255, 255);
            var gray = new Rgb24(191, 191, 191);

            var startTrigger = true;
            var grayTrigger = false;
            var blackTrigger = false;

            var lastPixel = CheckColor;
            var currentHour = 0;

            List<bool> values = Enumerable.Repeat(false, 24).ToList();

            for (int i = startWidthPixel; i < img.Width; i++)
            {
                var currentPixel = img[i, startHeightPixel];
                if (currentPixel == lastPixel) continue;
                lastPixel = currentPixel;

                if (startTrigger && currentPixel == CheckColor) continue;

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

                if (currentPixel == CheckColor && blackTrigger)
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

        private int FindHeightStartPixel(Image<Rgb24> image)
        {
            for (int i = image.Height - 1; i > 0; i--)
            {
                const int startWidthPixel = 30;
                var currentPixel = image[startWidthPixel, i];
                if (currentPixel == CheckColor) return i;
            }

            throw new Exception("Can't find start height pixel");
        }

        private int FindWidthStartPixel(Image<Rgb24> image, int startHeight)
        {
            for (int i = 0; i < image.Width; i++)
            {
                var currentPixel = image[i, startHeight];
                if (currentPixel == CheckColor) return i;
            }

            throw new Exception("Can't find start width pixel");
        }
    }
}
