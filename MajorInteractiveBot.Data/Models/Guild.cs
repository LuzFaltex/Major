using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MajorInteractiveBot.Data.Models
{
    public class Guild
    {
        [Key]
        public ulong GuildId { get; set; }
        public string CommandPrefix { get; set; }
        public bool GreetUser { get; set; }
        public ulong GreetingChannel { get; set; }
        public string GreetingMessage { get; set; }
        public ulong BotManager { get; set; }
        public ulong AdultRole { get; set; }
    }
}
