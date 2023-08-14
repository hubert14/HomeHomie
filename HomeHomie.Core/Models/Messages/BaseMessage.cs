using System.Text.Json;

namespace HomeHomie.Core.Models.Messages
{
    public abstract class BaseMessage
    {
        public abstract string Queue { get; }

        public string GetJson() => JsonSerializer.Serialize(this, GetType());
    }
}
