using HomeHomie.Core;

namespace HomeHomie.ElectricityModule.Models
{
    public class TelegramMessage
    {
        public string MessageId { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
    }

    public class ElectricityGraphic : BaseEntity
    {
        public string Date { get; set; } = string.Empty;
        public string? ImageLink { get; set; }

        public List<int> OnHours { get; set; } = new();
        public List<int> OffHours => Enumerable.Range(0, 23).Except(OnHours).ToList();

        public List<int> NotifiedHours { get; set; } = new();

        public List<TelegramMessage> Messages { get; set; } = new();

        public List<string> GetOnHoursSegments()
        {
            var segments = new List<string>();

            var lastState = false;
            var lastItem = 0;
            for (int i = 0; i < 24; i++)
            {
                void AddSegment()
                {
                    var str1 = lastItem.ToString();
                    if (lastItem < 10) str1 = "0" + str1;

                    var str2 = i.ToString();
                    if (i < 10) str2 = "0" + str2;

                    segments.Add($"{str1}:00-{str2}:00");
                }

                var state = OnHours.Contains(i);
                if (state != lastState)
                {
                    if (lastState)
                    {
                        AddSegment();
                    }

                    lastItem = i;
                }

                lastState = state;
                if (lastState && i == 23)
                {
                    AddSegment();
                }
            }

            return segments;
        }

        public override string ToString()
        {
            return ""
                + $"⏱ График на <b>{Date}</b>"
                + $"\n"
                + $"💡 Часы, в которые должен быть свет:"
                + $"\n"
                + $"<b>{string.Join(" | ", GetOnHoursSegments())}</b>";
        }

        public static ElectricityGraphic Parse(string text)
        {
            var lines = text.Split("\n");

            string? imageLink = null;
            string date;
            string[] hours;
            if (lines[0].StartsWith("http"))
            {
                imageLink = lines[0];
                date = lines[1];

                hours = lines.Length > 3
                    ? lines[2..]
                    : lines[2].Split(',').ToArray();
            }
            else
            {

                date = lines[0];

                hours = lines.Length > 2
                    ? lines[1..]
                    : lines[1].Split(',').ToArray();
            }

            return new ElectricityGraphic
            {
                ImageLink = imageLink,
                Date = date,
                OnHours = hours.Select(h => int.Parse(h)).ToList(),
                NotifiedHours = new()
            };
        }
    }
}
