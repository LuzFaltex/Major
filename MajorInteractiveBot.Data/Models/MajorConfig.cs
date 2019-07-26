using Microsoft.Extensions.Logging;

namespace MajorInteractiveBot.Data.Models
{
    public class MajorConfig
    {
        public string DiscordToken { get; set; }
        public string DbConnection { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public ulong BotOwner { get; set; } = 197291773133979648;
    }
}
