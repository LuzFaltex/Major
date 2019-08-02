using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MajorInteractiveBot.TypeReaders
{
    public class ModuleTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var commands = services.GetRequiredService<CommandService>();

            var foundModules = commands.Modules.Where(x => x.Name.Contains(input, StringComparison.OrdinalIgnoreCase));

            if (foundModules.Count() == 0)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"Could not find a module with the name '{input}'."));
            }
            else if (foundModules.Count() > 1)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, $"Found {string.Join(", ", foundModules)}"));
            }
            else // size is 1
            {
                var foundModule = foundModules.First();
                return Task.FromResult(TypeReaderResult.FromSuccess(foundModule));
            }
        }
    }
}
