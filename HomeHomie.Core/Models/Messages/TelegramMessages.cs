using HomeHomie.Core.Constants;

namespace HomeHomie.Core.Models.Messages
{
    public class TelegramMessages
    {
        public class SendTelegramMessageRequest : BaseMessage
        {
            public override string Queue => QueuesList.Telegram.SendTelegramMessageRequest;

            public string ChatId { get; }
            public string Message { get; }
            public string? MediaLink { get; }

            public SendTelegramMessageRequest(string chatId, string message)
            {
                ChatId = chatId;
                Message = message;
            }

            public SendTelegramMessageRequest(string chatId, string message, string mediaLink)
            {
                ChatId = chatId;
                Message = message;
                MediaLink = mediaLink;
            }
        }

        public class SendTelegramMessageResponse : BaseMessage
        {
            public override string Queue => QueuesList.Telegram.SendTelegramMessageResponse;

            public string ChatId { get; }
            public string MessageId { get; }

            public SendTelegramMessageResponse(string chatId, string messageId)
            {
                ChatId = chatId;
                MessageId = messageId;
            }
        }
    }
}
