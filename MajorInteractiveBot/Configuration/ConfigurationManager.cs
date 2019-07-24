using Discord;
using MajorInteractiveBot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Configuration
{
    public class ConfigurationManager
    {
        public static readonly string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "appconfig.json");        

        public static Task WriteConfig(object config, Formatting formatting = Formatting.Indented)
            => WriteConfig(config, ConfigPath, formatting);

        public static async Task WriteConfig(object config, string location, Formatting formatting = Formatting.Indented)
        {
            await File.WriteAllTextAsync(location, JsonConvert.SerializeObject(config, formatting));
        }

        public static Task<TResult> ReadConfig<TResult>()
            => ReadConfig<TResult>(ConfigPath);
        public static async Task<TResult> ReadConfig<TResult>(string location)
        {
            return JsonConvert.DeserializeObject<TResult>(await File.ReadAllTextAsync(location));
        }

        public static Task<Optional<TResult>> TryGetConfig<TResult>()
            => TryGetConfig<TResult>(ConfigPath);

        public static async Task<Optional<TResult>> TryGetConfig<TResult>(string location)
        {
            if (!File.Exists(ConfigPath))
            {
                return Optional.Create<TResult>();
            }
            else return await ReadConfig<TResult>(location);
        }
    }
}
