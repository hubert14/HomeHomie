using HomeHomie.Core.Constants;

namespace HomeHomie.Core.Models.Messages
{
    public class ElectricityGraphicMessages
    {
        public class SendNewGraphicRequestMessage : BaseMessage
        {
            public override string Queue => QueuesList.Electricity.SendNewGraphicRequestMessage;

            public string From { get; set; }
            public string Body { get; set; }

            public SendNewGraphicRequestMessage(string from, string body)
            {
                From = from;
                Body = body;
            }
        }

        public class SendGraphicRequestMessage : BaseMessage
        {
            public override string Queue => QueuesList.Electricity.SendGraphicRequestMessage;

            public string From { get; }

            public SendGraphicRequestMessage(string from)
            {
                From = from;
            }
        }

        public class SendGraphicResponseMessage : BaseMessage
        {
            public override string Queue => QueuesList.Electricity.SendGraphicResponseMessage;

            public string To { get; }
            public string? ImageLink { get; }
            public string Message { get; }

            public SendGraphicResponseMessage(string to, string message)
            {
                To = to;
                Message = message;
            }

            public SendGraphicResponseMessage(string to, string imageLink, string message)
            {
                To = to;
                ImageLink = imageLink;
                Message = message;
            }
        }
    }
    
}
