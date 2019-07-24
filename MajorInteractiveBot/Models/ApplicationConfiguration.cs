using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MajorInteractiveBot.Models
{
    [JsonObject]
    public class ApplicationConfiguration
    {

        [JsonIgnore]
        public string BotToken { get; } = System.Environment.GetEnvironmentVariable("MAJOR_BotToken");
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public Dictionary<ulong, GuildConfiguration> GuildConfigurations { get; set; } = new Dictionary<ulong, GuildConfiguration>();

        [JsonIgnore]
        public ulong BotOwner { get; } = 197291773133979648;

        public ApplicationConfiguration() : this(LogLevel.Information, new Dictionary<ulong, GuildConfiguration>())
        {
        }

        [JsonConstructor]
        public ApplicationConfiguration(LogLevel logLevel, Dictionary<ulong, GuildConfiguration> guildConfigurations)
        {
            LogLevel = logLevel;
            GuildConfigurations = guildConfigurations;
        }

        [JsonIgnore]
        public static readonly ApplicationConfiguration DefaultConfiguration 
            = new ApplicationConfiguration();
    }
}
