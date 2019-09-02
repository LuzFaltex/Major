using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    public class CommandHandler
    {
        #region properties
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly MajorContext _config;
        private readonly ILogger Log;        
        #endregion

        public CommandHandler(IServiceProvider services, MajorContext config, DiscordSocketClient client, CommandService commands, ILogger<CommandHandler> logger)
        {
            _services = services;
            _config = config;

            _client = client;
            _commands = commands;
            Log = logger;

            _client.MessageReceived += MessageReceived;
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore actions from bots
            if (!(rawMessage is SocketUserMessage message)) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Create command context
            var context = new SocketCommandContext(_client, message);
            var guild = await _config.Guilds.FindAsync(context.Guild.Id);
            var prefix = guild?.CommandPrefix ?? ".";

            if (prefix.Equals(".") && message.Content.StartsWith("..")) return;

            if (message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Execute command
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                Log.Log(
                    result.IsSuccess ? LogLevel.Debug : LogLevel.Warning, 
                    "{Username} ran command '{Command}' in Guild {Guild}", 
                    context.User.UsernameAndDiscrim(), context.Message, context.Guild.Name);

                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case CommandError.Exception:
                        case CommandError.ObjectNotFound:
                        case CommandError.ParseFailed:
                        case CommandError.UnmetPrecondition:
                        case CommandError.Unsuccessful:
                            await context.Channel.SendMessageAsync(result.ErrorReason);
                            Log.LogCritical(result.ErrorReason);
                            break;
                        case CommandError.BadArgCount:
                        case CommandError.UnknownCommand:
                            await message.AddReactionAsync(new Emoji("⚠"));
                            Log.LogWarning(result.ErrorReason);
                            break;
                    }
                }
            }
        }
    }
}
