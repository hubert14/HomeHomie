using HomeHomie.CacheModule;
using HomeHomie.DatabaseModule;
using HomeHomie.ElectricityModule;
using HomeHomie.HASSModule;
using HomeHomie.MessageBrokerModule;
using HomeHomie.TelegramModule;
using Microsoft.Extensions.Configuration;

namespace HomeHomie.Infrastructure
{
    public abstract class BaseSettings
    {
        protected IConfigurationSection Section { get; }

        public BaseSettings(IConfiguration configuration, bool simpleInit = true)
        {
            Section = configuration.GetSection(GetType().Name.Replace("Settings", ""));
            if (simpleInit) SimpleInit();
        }

        public BaseSettings(IConfigurationSection section, bool simpleInit = true)
        {
            Section = section;
            if (simpleInit) SimpleInit();
        }

        public BaseSettings(IConfiguration configuration, string sectionName, bool simpleInit = true)
        {
            Section = configuration.GetSection(sectionName);
            if (simpleInit) SimpleInit();
        }

        protected void SimpleInit()
        {
            foreach (var property in GetType().GetProperties())
            {
                property.SetValue(this, Section[property.Name]);
            }
        }
    }

    public class BrokerSettings : BaseSettings, IBrokerSettings
    {
        public BrokerSettings(IConfiguration config) : base(config)
        {
        }

        public string? HostName { get; private set; }
    }

    public class CacheSettings : BaseSettings, ICacheSettings
    {
        public CacheSettings(IConfiguration config) : base(config)
        {
        }

        public string? Address { get; private set; }
    }

    public class DatabaseSettings : BaseSettings, IDatabaseSettings
    {
        public DatabaseSettings(IConfiguration config) : base(config)
        {
        }

        public string? ConnectionString { get; private set; }

        public string? DatabaseName { get; private set; }
    }

    public class TelegramSettings : BaseSettings, ITelegramSettings
    {
        public TelegramSettings(IConfiguration config) : base(config)
        {
        }

        public string? ApiKey { get; private set; }
        public string[]? ChatsIds { get; private set; }
        public string? ServiceChatId { get; private set; }
    }

    public class ElectricitySettings : BaseSettings, IElectricitySettings
    {
        public ElectricitySettings(IConfiguration configuration) : base(configuration) { }

        public string? GraphicDomain { get; private set; }
        public string? GraphicPage { get; private set; }
        public int[]? DatesToCheck { get; private set; }
    }

    public class HomeAssistantSettings : BaseSettings, IHomeAssistantSettings
    {
        public HomeAssistantSettings(IConfiguration configuration) : base(configuration) { }

        public string? Address { get; private set; }
    }
}
