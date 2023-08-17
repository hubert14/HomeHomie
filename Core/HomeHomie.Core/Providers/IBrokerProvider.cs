using HomeHomie.Core.Models.Messages;

namespace HomeHomie.Core.Providers
{
    public interface IBrokerProvider
    {
        public void SendMessage(BaseMessage baseMessage);
        public Guid StartRecieving<T>(Func<T?, Task> asyncCallback) where T: BaseMessage;
        public void StopRecieving(Guid recieveKey);

    }
}
