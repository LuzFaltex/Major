using Discord.Commands;
using MajorInteractiveBot.Attributes;
using System.Linq;

namespace MajorInteractiveBot.Extensions
{
    public static class ModuleInfoExtensions
    {
        public static bool IsSystemModule(this ModuleInfo info)
        {
            if (info.Preconditions.Any(x => x is ModuleAttribute))
                return false;

            return true;
        }
    }
}
