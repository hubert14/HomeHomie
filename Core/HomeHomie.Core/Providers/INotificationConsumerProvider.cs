namespace HomeHomie.Core.Providers
{
    public interface INotificationConsumerProvider : IDisposable
    {
        public void StartReceiving();
    }
}
