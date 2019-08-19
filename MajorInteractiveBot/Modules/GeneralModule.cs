using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Extensions;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    [Name("General")]
    [Summary("General purpose commands")]
    public class GeneralModule : ModuleBase
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly MajorContext _dbContext;

        public GeneralModule(DiscordSocketClient discordClient, MajorContext dbContext)
        {
            _discordClient = discordClient;
            _dbContext = dbContext;
        }

        [Command("about")]
        [Summary("Provides information about the bot")]
        public async Task About()
        {
            var guildConfig = await _dbContext.Guilds.FindAsync(Context.Guild.Id);
            var embedBuilder = new EmbedBuilder()
            {
                Title = "Major",
                Url = "https://github.com/LuzFaltex/Major/",
                ThumbnailUrl = (Context.Client.CurrentUser as IUser).GetAvatarUrlOrDefault(),
                Description = $"Major is a Discord Interactive bot designed in collaboration with [Naka-Kon](https://naka-kon.com/) and [LuzFaltex](https://www.luzfaltex.com). It provides users with fun commands to improve their experience on the server.\n\nUse `{guildConfig.CommandPrefix}help` for a listing of commands.",
                Color = Color.Green
            }.WithDefaultFooter(Context.User);

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("ping")]
        [Summary("Ping MODiX to determine connectivity and latency")]
        public Task Ping()
            => ReplyAsync($"Pong! ({_discordClient.Latency} ms)");
    }
}
