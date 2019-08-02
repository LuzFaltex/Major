using Discord.Commands;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Attributes
{
    /// <summary>
    /// Flags a modules as a system module that can be disabled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ModuleAttribute : PreconditionAttribute
    {
        public ModuleAttribute()
        { }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var module = command.Module;

            if (module.IsSystemModule())
                return PreconditionResult.FromSuccess();

            var dbContext = services.GetRequiredService<MajorContext>();

            if (await dbContext.DisabledModules.FindAsync(context.Guild.Id, module.Group) is null)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("Module is disabled.");
        }
    }
}
