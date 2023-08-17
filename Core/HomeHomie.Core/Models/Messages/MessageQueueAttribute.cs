namespace HomeHomie.Core.Models.Messages
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class MessageQueueAttribute : Attribute
    {
        public string Queue { get; }
        public MessageQueueAttribute(string queue) => Queue = queue;
    }
}
