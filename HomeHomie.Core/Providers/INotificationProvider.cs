namespace HomeHomie.Core.Providers
{
    public interface INotificationProducerProvider : IDisposable
    {
        void StartProducing();
    }
}
