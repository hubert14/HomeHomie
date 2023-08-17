using HomeHomie.Core.Constants;

namespace HomeHomie.Core.Models.Messages
{
    public class TelegramMessages
    {
        [MessageQueue(QueuesList.Telegram.SendTelegramMessageRequest)]
        public class SendTelegramMessageRequest : BaseMessage
        {

            public string Message { get; set;  }

            public string? ChatId { get; set; }
            public string? MediaLink { get; set; }

            public bool IsServiceMessage { get; set; } = false;

            public ReplyToMessage[]? ReplyMessageIds { get; set; }

            public class ReplyToMessage
            {
                public string ChatId { get; set; }
                public string MessageId { get; set; }
            }
        }

        [MessageQueue(QueuesList.Telegram.SendTelegramMessageResponse)]
        public class SendTelegramMessageResponse : BaseMessage
        {
            public string ChatId { get; set; }
            public string MessageId { get; set;  }
        }
    }
}
