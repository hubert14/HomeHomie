using HomeHomie.Core.Interfaces;
using HomeHomie.Core.Models;
using HomeHomie.Core.Providers;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.PixelFormats;

namespace HomeHomie.ElectricityModule
{
    internal class ReportReciever : IPeriodWorker
    {
        private readonly IServiceScopeFactory _serviceScopeFac;

        private IElectricitySettings? _electricitySettings;

        public ReportReciever(IServiceScopeFactory serviceScopeFac) 
        {
            _serviceScopeFac = serviceScopeFac;
        }


        public IPeriodWorkerSettings Settings => new BasePeriodWorkerSettings(TimeSpan.FromMinutes(15));

        public async Task<bool> ProcessAsync()
        {
            using var scope = _serviceScopeFac.CreateScope();
            var database = scope.ServiceProvider.GetService<IDatabaseProvider>() ?? throw new ArgumentNullException($"{nameof(IDatabaseProvider)} is null");
            var broker = scope.ServiceProvider.GetService<IBrokerProvider>() ?? throw new ArgumentNullException($"{nameof(IBrokerProvider)} is null");
            var cache = scope.ServiceProvider.GetService<ICacheProvider>() ?? throw new ArgumentNullException($"{nameof(ICacheProvider)} is null");
            _electricitySettings = scope.ServiceProvider.GetService<IElectricitySettings>() ?? throw new ArgumentNullException($"{nameof(IElectricitySettings)} is null");

            throw new NotImplementedException();
        }

        //private async Task CheckGraphics(DateTime date)
        //{
        //    var stringDate = date.ToString("dd.MM.yyyy");
        //    var graphic = await MongoProvider.GetDataFromMongoAsync(stringDate);

        //    if (graphic != null) return;
        //    else
        //    {
        //        Console.WriteLine($"Try to pull graphic from website for date: " + stringDate);
        //        string imageLink;
        //        Stream imageStream;
        //        try
        //        {
        //            (imageLink, imageStream) = await PullGraphicAsync(date);
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message);
        //            return;
        //        }

        //        try
        //        {
        //            var data = ProcessImage(imageStream);
        //            var newGraphic = new ElectricityGraphic
        //            {
        //                Date = stringDate,
        //                ImageLink = imageLink,
        //                OnHours = data.Where(x => x.IsLightOn).Select(x => x.Hour).ToList()
        //            };


        //            var messages = await TelegramProvider.SendMediaMessageAsync(imageLink, newGraphic.ToString());
        //            newGraphic.Messages = messages;

        //            await MongoProvider.AddDataInMongoAsync(newGraphic);
        //        }
        //        catch (Exception e)
        //        {
        //            await TelegramProvider.SendMediaMessageAsync(imageLink, e.Message, SERVICE_CHAT_ID);
        //        }
        //    }
        //}

        private async Task<(string imageLink, Stream imageStream)> PullGraphicAsync(DateTime date)
        {
            var httpClient = new HttpClient();
            var siteUrl = "https://" + _electricitySettings.GraphicDomain;
            var html = await httpClient.GetStringAsync(siteUrl + "/" + _electricitySettings.GraphicPage);

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var values = document.DocumentNode
                .SelectNodes("//img")
                .FirstOrDefault(x => x.GetAttributeValue("src", "~unknown").Contains("current-timetable"));

            var url = values.GetAttributeValue("src", "~unknown");

            var imageName = url.Split('/').Last();
            var today = date.ToString("ddMMyy");
            var nextDay = date.AddDays(1).ToString("yyyyMMdd");
            var twoDays = date.Day + "_" + date.AddDays(1).ToString("ddMMyy");
            var previousAndToday = date.AddDays(-1).Day + "_" + date.AddDays(1).ToString("ddMMyy");

            if (!imageName.Contains(today) && !imageName.Contains(nextDay) && !imageName.Contains(twoDays)) throw new Exception("This graphic not for requested date. Graphic name: " + imageName + " | Date: " + date.ToString("dd.MM.yy"));

            return (siteUrl + url, await httpClient.GetStreamAsync(siteUrl + url));
        }

        private List<(int Hour, bool IsLightOn)> ProcessImage(Stream imageStream)
        {
            using var img = Image.Load<Rgb24>(imageStream);

            var heigth = img.Height;
            var startHeightPixel = FindHeightStartPixel(img);
            var startWidthPixel = FindWidthStartPixel(img, startHeightPixel);

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

            for (int i = startWidthPixel; i < img.Width; i++)
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

        private int FindHeightStartPixel(Image<Rgb24> image)
        {
            var blue = new Rgb24(0, 176, 240);
            for (int i = image.Height - 1; i > 0; i--)
            {
                const int startWidthPixel = 30;
                var currentPixel = image[startWidthPixel, i];
                if (currentPixel == blue) return i;
            }

            throw new Exception("Can't find start height pixel");
        }

        private int FindWidthStartPixel(Image<Rgb24> image, int startHeight)
        {
            var blue = new Rgb24(0, 176, 240);
            for (int i = 0; i < image.Width; i++)
            {
                var currentPixel = image[i, startHeight];
                if (currentPixel == blue) return i;
            }

            throw new Exception("Can't find start width pixel");
        }
    }
}
