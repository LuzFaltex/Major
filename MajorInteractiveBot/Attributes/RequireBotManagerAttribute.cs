using Discord.Commands;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Attributes
{
    [AttributeUsage(AttributeTargets.Method| AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RequireBotManagerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var dbContext = services.GetRequiredService<MajorContext>();
            var options = services.GetRequiredService<IOptions<MajorConfig>>().Value;

            var userId = context.User.Id;
            var guild = await dbContext.Guilds.FindAsync(context.Guild.Id);

            if (userId.Equals(guild.BotManager) || userId.Equals(context.Guild.OwnerId) || userId.Equals(options.BotOwner))
            {
                return PreconditionResult.FromSuccess();
            }
            else return PreconditionResult.FromError("You are not authorized to perform this action.");
        }
    }
}
