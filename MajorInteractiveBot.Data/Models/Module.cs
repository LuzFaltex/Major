using System.ComponentModel.DataAnnotations;

namespace MajorInteractiveBot.Data.Models
{
    public class Module
    {
        public string ModuleName { get; set; }
        public ulong GuildId { get; set; }
    }
}
