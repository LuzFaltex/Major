using Discord.Commands;
using MajorInteractiveBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    public class GeneralCommands : ModuleBase
    {
        private readonly ApplicationConfiguration _appConfig;

        public GeneralCommands(ApplicationConfiguration appConfig)
        {
            _appConfig = appConfig;
        }

        [Command("ping")]
        public async Task SayHello()
        {
            await ReplyAsync($"Hello World! You're from {Context.Guild.Name}.");
        }
    }
}
