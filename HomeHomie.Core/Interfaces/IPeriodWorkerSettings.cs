namespace HomeHomie.Core.Interfaces
{
    public interface IPeriodWorkerSettings
    {
        public TimeSpan Period { get; set; }

        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
