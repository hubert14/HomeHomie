using HomeHomie.Core.Providers;
using HomeHomie.ElectricityModule.Models;
using static HomeHomie.Core.Models.Messages.ElectricityGraphicMessages;
using static HomeHomie.Core.Models.Messages.TelegramMessages;

namespace HomeHomie.ElectricityModule
{
    internal class ReportSaver : IDisposable
    {
        private IDatabaseProvider _database;
        private IBrokerProvider _broker;

        private readonly Guid _newUserGraphicRecieverKey;

        public ReportSaver(IDatabaseProvider database, IBrokerProvider broker)
        {
            _database = database;
            _broker = broker;

            _newUserGraphicRecieverKey = _broker.StartRecieving<SendNewGraphicRequestMessage>(SaveUserGraphicAsync);
        }

        private async Task SaveUserGraphicAsync(SendNewGraphicRequestMessage? message)
        {
            if (message is null) throw new Exception($"{nameof(message)} is null");

            var parsedGraphic = ElectricityGraphic.Parse(message.Body!);

            var existedGraphic = _database.Get<ElectricityGraphic>()
                .Where(x => x.Date == parsedGraphic.Date).FirstOrDefault();

            if (existedGraphic is null)
            {
                await _database.InsertAsync(parsedGraphic);
            }
            else
            {
                existedGraphic.OnHours = parsedGraphic.OnHours;
                existedGraphic.UpdatedAt = DateTime.Now;
                await _database.ReplaceAsync(existedGraphic);
            }

            const string message_text = "Записал, спасибо!";
            _broker.SendMessage(new SendTelegramMessageRequest { Message = message_text, ChatId = message.From });

            const string MessageHeader = "График был обновлен в ручном режиме!\n";
            _broker.SendMessage(new SendTelegramMessageRequest
            {
                Message = MessageHeader,
                MediaLink = existedGraphic?.ImageLink,
                ReplyMessageIds = existedGraphic?.Messages.Select(x => new SendTelegramMessageRequest.ReplyToMessage { ChatId = x.ChatId, MessageId = x.MessageId }).ToArray()
            });
        }

        public void Dispose()
        {
            _broker.StopRecieving(_newUserGraphicRecieverKey);
        }
    }
}
