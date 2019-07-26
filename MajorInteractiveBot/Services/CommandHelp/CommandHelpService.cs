using Discord.Commands;
using MajorInteractiveBot.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MajorInteractiveBot.Services.CommandHelp
{
    /// <summary>
    /// Provides functionality to retrieve command help information.
    /// </summary>
    public interface ICommandHelpService
    {
        /// <summary>
        /// Retrieves help data for all available modules.
        /// </summary>
        /// <returns>
        /// A readonly collection of data about all available modules.
        /// </returns>
        IReadOnlyCollection<ModuleHelpData> GetModuleHelpData();

        /// <summary>
        /// Retrieves help data for the supplied query.
        /// </summary>
        /// <param name="query">A query to use to search for an applicable help module.</param>
        /// <returns>
        /// Help information for the supplied query, or <see langword="null"/> if no information could be found for the supplied query.
        /// </returns>
        ModuleHelpData GetModuleHelpData(string query);
    }

    public class CommandHelpService : ICommandHelpService
    {
        private readonly CommandService _commands;
        private IReadOnlyCollection<ModuleHelpData> _cachedHelpData;

        public CommandHelpService(CommandService commandService)
        {
            _commands = commandService;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ModuleHelpData> GetModuleHelpData()
            => LazyInitializer.EnsureInitialized(ref _cachedHelpData, () =>
            _commands.Modules
            .Where(x => !x.Attributes.Any(attr => attr is HiddenFromHelpAttribute))
            .Select(x => ModuleHelpData.FromModuleInfo(x))
            .ToArray());

        /// <inheritdoc />
        public ModuleHelpData GetModuleHelpData(string query)
        {
            var allHelpData = GetModuleHelpData();

            var byNameExact = allHelpData.FirstOrDefault(x => x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (byNameExact != null)
                return byNameExact;

            var byTagsExact = allHelpData.FirstOrDefault(x => x.HelpTags.Any(y => y.Equals(query, StringComparison.OrdinalIgnoreCase)));
            if (byTagsExact != null)
                return byTagsExact;

            var byNameContains = allHelpData.FirstOrDefault(x => x.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            if (byNameContains != null)
                return byNameContains;

            var byTagsContains = allHelpData.FirstOrDefault(x => x.HelpTags.Any(y => y.Contains(query, StringComparison.OrdinalIgnoreCase)));
            if (byNameContains != null)
                return byTagsContains;

            return null;
        }
    }
}
