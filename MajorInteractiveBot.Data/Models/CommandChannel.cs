using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MajorInteractiveBot.Data.Models
{
    public class CommandChannel
    {
        [Key]
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
    }
}
