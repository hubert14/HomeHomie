using HomeHomie.Core.Models.Messages;
using HomeHomie.Core.Providers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace HomeHomie.MessageBrokerModule
{
    internal class RabbitMQBroker : IBrokerProvider, IDisposable
    {
        private readonly IConnection _connection;
        private readonly List<(string Queue, bool IsConsumer, IModel Channel)> _channels = new();

        private readonly Dictionary<Guid, EventingBasicConsumer> Consumers = new();

        public RabbitMQBroker(IBrokerSettings settings)
        {
            var factory = new ConnectionFactory { HostName = settings.HostName, };
            _connection = factory.CreateConnection();
        }

        public Guid StartRecieving<T>(string queue, Func<T?, Task> asyncCallback)
        {
            var channel = EnsureModel(queue, true);
            channel.QueueDeclare(queue: queue,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            var recieveKey = Guid.NewGuid();
            Consumers.Add(recieveKey, consumer);

            channel.BasicConsume(queue, true, consumer);
            consumer.Received += async (s, args) =>
            {
                var str = Encoding.UTF8.GetString(args.Body.ToArray());
                var result = JsonSerializer.Deserialize<T>(str);
                await asyncCallback.Invoke(result);
            };

            return recieveKey;
        }

        public void SendMessage(BaseMessage message)
        {
            var channel = EnsureModel(message.Queue, false);
            channel.QueueDeclare(queue: message.Queue,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            channel.BasicPublish(exchange: "", routingKey: message.Queue, null, body: Encoding.UTF8.GetBytes(message.GetJson()));
        }

        private IModel EnsureModel(string queue, bool isConsumer)
        {
            IModel channel;
            if (_channels.Any(x => x.Queue == queue && x.IsConsumer == isConsumer))
            {
                channel = _channels.First(x => x.Queue == queue && x.IsConsumer == isConsumer).Channel;
            }
            else
            {
                channel = _connection.CreateModel();
                _channels.Add((queue, isConsumer, channel));
            }

            return channel;
        }

        public void Dispose()
        {
            _channels.ForEach(x => x.Channel.Dispose());
            _channels.Clear();
            _connection.Dispose();
        }

        // TODO: Check the stop recieving (possibly there is another simplier way)
        public void StopRecieving(Guid recieveKey)
        {
            if (!Consumers.TryGetValue(recieveKey, out var consumer)) return;
            consumer.ClearEventInvocations(nameof(consumer.Received));
        }
    }

    public static class EventExtensions
    {
        public static void ClearEventInvocations(this object obj, string eventName)
        {
            var fi = obj.GetType().GetEventField(eventName);
            if (fi == null) return;
            fi.SetValue(obj, null);
        }

        private static FieldInfo? GetEventField(this Type? type, string eventName)
        {
            FieldInfo? field = null;
            while (type != null)
            {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }
            return field;
        }

    }
}
