using MajorInteractiveBot.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MajorInteractiveBot.Data
{
    public class MajorContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<CommandChannel> CommandChannels { get; set; }
        public DbSet<Module> Modules { get; set; }

        public MajorContext(DbContextOptions<MajorContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Module>().HasKey(t => new { t.GuildId, t.ModuleName });
        }
    }
}
