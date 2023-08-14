using HomeHomie.Core.Providers;
using HomeHomie.TelegramModule.Telegram;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HomeHomie.TelegramModule
{
    public static class DI
    {
        public static IServiceCollection AddNotificationProducer(this IServiceCollection services)
        {
            return services.AddSingleton<INotificationProducerProvider, TelegramProvider>();
        }
    }
}
