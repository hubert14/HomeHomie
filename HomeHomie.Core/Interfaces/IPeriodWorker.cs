namespace HomeHomie.Core.Interfaces
{
    public interface IPeriodWorker
    {
        public IPeriodWorkerSettings Settings { get; }

        public Task<bool> ProcessAsync();
    }
}
