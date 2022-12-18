using MongoDB.Driver;

namespace HomeHomie.Database.Entities
{
    public class ElectricityGraphic : BaseEntity
    {
        public static string CollectionName => "electricityGraphic";

        public string Date { get; set; }
        public string ImageLink { get; set; }

        public List<int> OnHours { get; set; } = new();
        public List<int> OffHours => Enumerable.Range(0, 23).Except(OnHours).ToList();

        public List<int> NotifiedHours { get; set; } = new();


        public List<string> GetOnHoursSegments()
        {
            var segments = new List<string>();

            var lastState = false;
            var lastItem = 0;
            for (int i = 0; i < 24; i++)
            {
                var state = OnHours.Contains(i);
                if (state != lastState)
                {
                    if (lastState)
                    {
                        var str1 = lastItem.ToString();
                        if (lastItem < 10) str1 = "0" + str1;

                        var str2 = i.ToString();
                        if (i < 10) str2 = "0" + str2;

                        segments.Add($"{str1}:00-{str2}:00");
                    }

                    lastItem = i;
                }

                lastState = state;
            }

            if (OnHours.Contains(23))
            {
                segments.Add("23:00");
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
            var date = lines[0];

            var hours = lines.Length > 2
                ? lines[1..]
                : lines[1].Split(',').ToArray();

            return new ElectricityGraphic
            {
                Date = date,
                OnHours = hours.Select(h => int.Parse(h)).ToList(),
                NotifiedHours = new()
            };
        }
    }
}
