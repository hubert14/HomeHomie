using HomeHomie.Core.Interfaces;

namespace HomeHomie.Core.Models
{
    public class BasePeriodWorkerSettings : IPeriodWorkerSettings
    {
        public TimeSpan Period { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        public BasePeriodWorkerSettings(TimeSpan period) { Period = period; }
        public BasePeriodWorkerSettings(TimeSpan period, DateTime startAt) { Period = period; StartAt = startAt; }
        public BasePeriodWorkerSettings(TimeSpan period, DateTime startAt, DateTime endAt)
        {
            Period = period;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
