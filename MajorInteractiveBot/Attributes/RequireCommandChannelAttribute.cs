using Discord.Commands;
using MajorInteractiveBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited =true)]
    public sealed class RequireCommandChannelAttribute : PreconditionAttribute
    {
        public List<ulong> AllowedChannels { get; }
        public bool IncludeConfigValues { get; }

        /// <summary>
        /// Require that the channel the command is being invoked in is authorized as per the configuration
        /// </summary>
        public RequireCommandChannelAttribute() : this(true, new ulong[0]) { }

        /// <summary>
        /// Require that the channel the command is being invoked in is one of these
        /// </summary>
        /// <param name="includeConfigValues">Whether to include those defined in the guild config</param>
        /// <param name="channels">A list of additional channels</param>
        public RequireCommandChannelAttribute(bool includeConfigValues, params ulong[] channels)
        {
            AllowedChannels = channels.ToList();
            IncludeConfigValues = includeConfigValues;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var appConfig = services.GetRequiredService<ApplicationConfiguration>();

            if (IncludeConfigValues)
            {
                var tempCollection = appConfig.GuildConfigurations[context.Guild.Id].CommandChannels;
                AllowedChannels.AddRange(tempCollection);
            }

            if (context.User.Id == appConfig.BotOwner || AllowedChannels.Count == 0 || AllowedChannels.Any(x => x == context.Channel.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Invalid Channel"));
            }
        }
    }
}
