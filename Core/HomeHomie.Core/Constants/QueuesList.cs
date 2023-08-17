namespace HomeHomie.Core.Constants
{
    public static class QueuesList
    {
        public static class Telegram
        {
            public const string SendTelegramMessageRequest = "telegram-send-message-request";
            public const string SendTelegramMessageResponse = "telegram-send-message-response";
        }

        public static class Electricity
        {
            public const string SendNewUserGraphicMessage = "electricity-user-graphic-new";
            public const string SendGraphicRequestMessage = "electricity-graphic-request";
        }
    }
}
