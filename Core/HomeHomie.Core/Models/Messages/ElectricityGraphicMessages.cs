using HomeHomie.Core.Constants;

namespace HomeHomie.Core.Models.Messages
{
    public class ElectricityGraphicMessages
    {
        [MessageQueue(QueuesList.Electricity.SendNewUserGraphicMessage)]
        public class SendNewGraphicRequestMessage : BaseMessage
        {
            public string From { get; set; }
            public string Body { get; set; }
        }

        [MessageQueue(QueuesList.Electricity.SendGraphicRequestMessage)]
        public class SendGraphicRequestMessage : BaseMessage
        {
            public string From { get; set; }
        }
    }
}
