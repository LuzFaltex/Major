using Microsoft.Extensions.Logging;

namespace MajorInteractiveBot.Models
{
    public class ApplicationConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public ulong BotOwner { get; set; } = 197291773133979648;
        public string SqlLiteDBPath { get; set; }
    }
}
