using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    [Name("General")]
    [Group("general")]
    [Summary("General purpose commands")]
    public class GeneralModule : ModuleBase
    {
        private readonly DiscordSocketClient _discordClient;

        public GeneralModule(DiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
        }

        [Command("ping")]
        [Summary("Ping MODiX to determine connectivity and latency")]
        public Task Ping()
            => ReplyAsync($"Pong! ({_discordClient.Latency} ms)");
    }
}
