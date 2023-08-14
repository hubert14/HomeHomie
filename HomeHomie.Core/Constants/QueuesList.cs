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
            public const string SendNewGraphicRequestMessage = "electricity-graphic-new";

            public const string SendGraphicRequestMessage = "electricity-graphic-request";
            public const string SendGraphicResponseMessage = "electricity-graphic-response";
        }

    }
}
