using HomeHomie.Core.Models.Messages;

namespace HomeHomie.Core.Providers
{
    public interface IBrokerProvider
    {
        public void SendMessage(BaseMessage baseMessage);
        public Guid StartRecieving<T>(string queue, Func<T?, Task> asyncCallback);
        public void StopRecieving(Guid recieveKey);

    }
}
