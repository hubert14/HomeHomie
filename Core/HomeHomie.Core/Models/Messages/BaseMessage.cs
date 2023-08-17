using System.Text.Json;

namespace HomeHomie.Core.Models.Messages
{
    public abstract class BaseMessage
    {
        public string GetJson() => JsonSerializer.Serialize(this, GetType());
    }
}
