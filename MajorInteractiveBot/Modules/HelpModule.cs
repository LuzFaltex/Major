﻿using Discord;
using Discord.Commands;
using Discord.Net;
using MajorInteractiveBot.Attributes;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Services.CommandHelp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    [Name("Help")]
    [Group("help")]
    [Summary("Provides commands for helping users understand how to interact with General.")]
    public class HelpModule : ModuleBase
    {
        private readonly ICommandHelpService _commandHelpService;
        private readonly MajorContext _config;

        public HelpModule(ICommandHelpService commandHelpService, MajorContext config)
        {
            _commandHelpService = commandHelpService;
            _config = config;
        }

        [Command]
        [Summary("Prints a neat list of all commands.")]
        public async Task HelpAsync()
        {
            // var guildConfig = _config.GuildConfigurations[Context.Guild.Id];
            var modules = _commandHelpService.GetModuleHelpData();
                // .Select(d => d.Name)
                // .Where(x => guildConfig.IsModuleEnabled(x))
                // .OrderBy(d => d);

            var descriptionBuilder = new StringBuilder()
                .AppendLine("Modules:")
                .AppendJoin(", ", modules.Select(d => d.Name))
                .AppendLine()
                .AppendLine()
                .AppendLine($"Do `.help dm` to have everything DMed to you. (Spammy!)")
                .AppendLine($"Do `.help [module name]` to have that module's commands listed.");

            var embed = new EmbedBuilder()
                .WithTitle("Help")
                .WithDescription(descriptionBuilder.ToString());

            await ReplyAsync(embed: embed.Build());
        }

        [Command("dm")]
        [Summary("Spams the user's DMs with a list of every command available.")]
        public async Task HelpDMAsync()
        {
            var userDM = await Context.User.GetOrCreateDMChannelAsync();

            foreach(var module in _commandHelpService.GetModuleHelpData().OrderBy(x => x.Name))
            {
                var embed = GetEmbedForModule(module);

                try
                {
                    await userDM.SendMessageAsync(embed: embed.Build());
                } catch (HttpException ex) when (ex.DiscordCode == 50007)
                {
                    await ReplyAsync($"You have private messages for this server disabled, {Context.User.Mention}. Please enable them so that I can send you help.");
                    return;
                }
            }

            await ReplyAsync("📬");
        }

        [Command]
        [Summary("Prints a neat list of all commands based on the supplied query.")]
        [Priority(-10)]
        public async Task HelpAsync(
            [Remainder]
            [Summary("The module name or related query to use to search for the help module.")]
            string query)
        {
            
            var foundModule = _commandHelpService.GetModuleHelpData(query);

            if (foundModule is null)
            {
                await ReplyAsync($"Sorry, I couldn't find help related to `{query}`");
                return;
            }

            var module = _config.DisabledModules.FirstOrDefault(x => x.ModuleName == foundModule.Name && x.GuildId == Context.Guild.Id);
            if (!(module is null))
            {
                await ReplyAsync($"Sorry, the selected module is disabled. If you believe this to be in error, please contact an administrator.");
                return;
            }
            
            var embed = GetEmbedForModule(foundModule);

            await ReplyAsync($"Results for `{query}`:", embed: embed.Build());
        }

        private EmbedBuilder GetEmbedForModule(ModuleHelpData module)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Module: {module.Name}")
                .WithDescription(module.Summary);

            return AddCommandFields(embedBuilder, module.Commands);
        }

        private EmbedBuilder AddCommandFields(EmbedBuilder embedBuilder, IEnumerable<CommandHelpData> commands)
        {

            var guildConfig = _config.Guilds.Find(Context.Guild.Id);

            foreach (var command in commands)
            {
                var summaryBuilder = new StringBuilder(command.Summary ?? "No Summary.").AppendLine();
                var summary = AppendAliases(summaryBuilder, command.Aliases);

                embedBuilder.AddField(new EmbedFieldBuilder()
                    .WithName($"Command: {guildConfig.CommandPrefix}{command.Aliases.FirstOrDefault()} {GetParams(command)}")
                    .WithValue(summary.ToString()));
            }
            
            return embedBuilder;
        }

        private StringBuilder AppendAliases(StringBuilder stringBuilder, IReadOnlyCollection<string> aliases)
        {
            if (aliases.Count == 0)
                return stringBuilder;

            stringBuilder.AppendLine(Format.Bold("Aliases:"));

            // foreach (var alias in FormatUtilities.CollapsePlurals(aliases))
            foreach (var alias in aliases)
            {
                stringBuilder.AppendLine($"• {alias}");
            }

            return stringBuilder;
        }

        private string GetParams(CommandHelpData info)
        {
            var sb = new StringBuilder();

            foreach (var parameter in info.Parameters)
            {
                if (parameter.IsOptional)
                    sb.Append($"[Optional({parameter.Name})]");
                else
                    sb.Append($"[{parameter.Name}]");
            }

            return sb.ToString();
        }
    }
}
