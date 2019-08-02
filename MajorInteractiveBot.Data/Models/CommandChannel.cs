using System.ComponentModel.DataAnnotations;

namespace MajorInteractiveBot.Data.Models
{
    public class CommandChannel
    {
        [Key]
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
    }
}
