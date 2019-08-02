using System.ComponentModel.DataAnnotations;

namespace MajorInteractiveBot.Data.Models
{
    public class AssignableRole
    {
        [Key]
        public ulong RoleId { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleCategory { get; set; }
        public bool Require18Plus { get; set; }
        public int Position { get; set; }
    }
}
