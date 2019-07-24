using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MajorInteractiveBot.Models
{
    public class GuildConfiguration
    {
        public ulong GuildId { get; }
        public string CommandPrefix { get; set; } = ".";

        private readonly HashSet<ulong> _commandChannels = new HashSet<ulong>();
        [JsonIgnore]
        public IReadOnlyCollection<ulong> CommandChannels { get { return _commandChannels; } }

        public GuildConfiguration(IGuild guild)
        {
            GuildId = guild.Id;
        }

        private readonly HashSet<string> _disabledModules = new HashSet<string>();
        [JsonIgnore]
        public IReadOnlyCollection<string> DisabledModules { get { return _disabledModules; } }

        private readonly Dictionary<string, string> _tags = new Dictionary<string, string>();
        [JsonIgnore]
        public IReadOnlyDictionary<string, string> Tags = new Dictionary<string, string>();

        public bool GreetUser { get; set; } = false;
        public ulong GreetingChannel { get; set; } = ulong.MaxValue;
        public string GreetingMessage { get; set; } = "Welcome to {Guild}, {UserMention}!";

        [JsonConstructor]
        public GuildConfiguration(ulong guildId, string commandPrefix, ICollection<ulong> commandChannels)
        {
            GuildId = guildId;
            CommandPrefix = commandPrefix;
            _commandChannels = new HashSet<ulong>(commandChannels);
        }

        /// <summary>
        /// Determines whether the provided channel is a CommandChannel.
        /// </summary>
        /// <param name="channel">The channel to test.</param>
        /// <returns>True if the provided channel lives in the command channel collection or if the collection is empty; otherwise, false.</returns>
        public bool IsCommandChannel(IGuildChannel channel)
        {
            if (_commandChannels.Count == 0) return true;
            return _commandChannels.Contains(channel.Id);
        }

        /// <summary>
        /// Registers the provided channel as a command channel.
        /// </summary>
        /// <param name="channel">The channel to register.</param>
        /// <returns>True if the operation succeeded; otherwise, false.</returns>
        public bool RegisterCommandChannel(IGuildChannel channel)
        {
            return _commandChannels.Add(channel.Id);
        }

        /// <summary>
        /// Unregisters the specified channel as a command channel.
        /// </summary>
        /// <param name="id">The ID of the channel</param>
        /// <returns>True if the operation succeeded; otherwise, false.</returns>
        public bool UnregisterCommandChannel(ulong id)
        {
            return _commandChannels.Remove(id);
        }

        public bool DisableModule(string moduleName)
        {
            return _disabledModules.Add(moduleName);
        }

        public bool EnableModule(string moduleName)
        {
            return _disabledModules.Remove(moduleName);
        }

        public bool IsModuleEnabled(string moduleName)
        {
            return !_disabledModules.Contains(moduleName);
        }

        /// <summary>
        /// Adds a new tag to the tag store.
        /// </summary>
        /// <param name="key">The name of the tag.</param>
        /// <param name="value">The text of the tag.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public bool AddTag(string key, string value)
        {
            return _tags.TryAdd(key, value);
        }

        /// <summary>
        /// Removes the specified tag from the tag store.
        /// </summary>
        /// <param name="key">The name of the tag to remove.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public bool RemoveTag(string key)
        {
            return _tags.Remove(key);
        }

        /// <summary>
        /// Modifies an existing tag in the tag store.
        /// </summary>
        /// <param name="key">The name of the key to edit.</param>
        /// <param name="value">The new text of the tag.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        public bool UpdateTag(string key, string value)
        {
            if (!_tags.ContainsKey(key))
                return false;

            _tags[key] = value;
            return true;
        }

        public string FormatGreetingMessage(IGuildUser user, params string[] args)
        {
            string message = GreetingMessage
                .Replace("{Guild}", user.Guild.Name)
                .Replace("{User}", user.Username)
                .Replace("{UserMention}", user.Mention);

            return string.Format(message, args);
        }
    }
}
