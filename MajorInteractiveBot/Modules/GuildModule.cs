using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using MajorInteractiveBot.Attributes;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Extensions;
using MajorInteractiveBot.Services.CommandHelp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    [Name("Guild")]
    [Group("guild")]
    [Summary("Provides commands for the user to manage the guild configuration")]
    [RequireBotManager]
    public class GuildModule : ModuleBase
    {
        private readonly MajorContext _context;

        public GuildModule(MajorContext context)
        {
            _context = context;
        }

        [Command("info")]
        [Summary("Provides a listing of details related to the current guild.")]
        public async Task GetGuildInfo()
        {
            var guild = await _context.Guilds.FindAsync(Context.Guild.Id);

            var fields = new List<EmbedFieldBuilder>()
            {
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = nameof(guild.GuildId).Humanize(LetterCasing.Title),
                    Value = guild.GuildId
                },
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = nameof(guild.CommandPrefix).Humanize(LetterCasing.Title),
                    Value = guild.CommandPrefix
                },
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = nameof(guild.GreetUser).Humanize(LetterCasing.Title),
                    Value = guild.GreetUser
                },
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = nameof(guild.GreetingChannel).Humanize(LetterCasing.Title),
                    Value = guild.GreetingChannel == 0 ? "Undefined" : (await Context.Guild.GetChannelAsync(guild.GreetingChannel)).Name + " (" + guild.GreetingChannel + ")"
                },
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = nameof(guild.GreetingMessage).Humanize(LetterCasing.Title),
                    Value = string.IsNullOrWhiteSpace(guild.GreetingMessage) ? "Undefined" : guild.GreetingMessage
                },
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = nameof(guild.BotManager).Humanize(LetterCasing.Title),
                    Value = guild.BotManager == 0 ? "Undefined" : Context.Guild.GetRole(guild.BotManager).Name + " (" + guild.BotManager + ")"
                },
                
                // Modules are listed under another command
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = "Modules",
                    Value = $"See {guild.CommandPrefix}guild modules"
                },

                // Command channels are too
                new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = "Command Channels",
                    Value = $"See {guild.CommandPrefix}guild cc"
                }
            };

            var embedBuilder = new EmbedBuilder()
            {
                Title = Context.Guild.Name,
                ThumbnailUrl = Context.Guild.IconUrl,
                Color = Color.Green,
                Description = "Guild Information",
                Fields = fields,
                Timestamp = DateTime.UtcNow
            }.WithDefaultFooter(Context.User);

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("prefix")]
        [Summary("Gets the current bot prefix.")]
        public async Task GetPrefix()
            => await Context.ReplyWithEmbed($"Current prefix: {(await _context.Guilds.FindAsync(Context.Guild.Id)).CommandPrefix}");

        [Command("prefix")]
        [Summary("Sets the current bot prefix to the provided value.")]
        [Priority(-10)]
        public async Task SetPrefix([NotNullOrWhiteSpace][Summary("The new prefix for this bot to use")] string newPrefix)
        {
            var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
            var oldPrefix = guild.CommandPrefix;

            if (guild.CommandPrefix.Equals(newPrefix))
            {
                await Context.ReplyWithEmbed($"Guild prefix was already '{oldPrefix}'. No changes were made.");
            }
            else
            {
                guild.CommandPrefix = newPrefix;
                await _context.SaveChangesAsync();
                // await ReplyAsync($"Guild prefix updated: {oldPrefix} -> {guild.CommandPrefix}");
                await Context.AddConfirmation();
            }            
        }

        [Command("manager")]
        [Alias("botManager")]
        [Summary("Gets the current bot manager role.")]
        public async Task GetManager()
        {
            var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
            var currentManagerRole = guild.BotManager == 0 ? "Undefined" : Context.Guild.GetRole(guild.BotManager).NameAndId();
            await Context.ReplyWithEmbed($"Current Bot Manager Role: {currentManagerRole}");
        }

        [Command("manager")]
        [Alias("botManager")]
        [Summary("Manage the current bot manager role.")]
        public async Task SetManager(IRole role)
        {
            var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
            var currentManagerRole = guild.BotManager == 0 ? "Undefined" : Context.Guild.GetRole(guild.BotManager).NameAndId();

            if (guild.BotManager.Equals(role.Id))
            {
                await Context.ReplyWithEmbed($"Bot manager role was already '{currentManagerRole}'. No changes were made.");
            }
            else
            {
                guild.BotManager = role.Id;
                await _context.SaveChangesAsync();
                // await ReplyAsync($"Bot manager role updated: {currentManagerRole} -> {role.NameAndId()}");
                await Context.AddConfirmation();
            }
        }

        [Name("Greeting")]
        [Group("greeting")]
        [Summary("Manage the greeting module")]
        [RequireBotManager]
        public class GreetingModule : InteractiveBase
        {
            private readonly MajorContext _context;
            private readonly IHostEnvironment _environment;
            private readonly IServiceProvider _services;

            public GreetingModule(MajorContext context, IHostEnvironment environment, IServiceProvider services)
            {
                _context = context;
                _environment = environment;
                _services = services;
            }

            [Command]
            [Summary("Returns whether the bot is currently configured to greet the user.")]
            public async Task GetGreetUser()
            {
                var guild = await _context.Guilds.FindAsync(Context.Guild.Id);

                var message = $"I am currently {(guild.GreetUser ? "greeting" : "ignoring")} new users.";

                if (_environment.IsDevelopment())
                    message += Environment.NewLine + $"({nameof(guild.GreetUser)} = {guild.GreetUser})";

                await Context.ReplyWithEmbed(message);
            }

            [Command]
            [Summary("Allows the user to toggle user greeting.")]
            public async Task SetGreetUser([Summary("True if the bot should greet new users; otherwise, false")] bool shouldGreet)
            {
                var guild = await _context.Guilds.FindAsync(Context.Guild.Id);

                if (guild.GreetUser.Equals(shouldGreet))
                    await Context.ReplyWithEmbed($"I was already {(shouldGreet ? "greeting" : "ignoring")} new users. No changes were made.");
                else
                {
                    guild.GreetUser = shouldGreet;
                    await _context.SaveChangesAsync();
                    await Context.ReplyWithEmbed($"I will now {(shouldGreet ? "greet" : "ignore")} new users.");
                }
            }

            [Command("message")]
            [Summary("Returns the current greeting message.")]
            public async Task GetGreetingMessage()
            {
                var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
                var currentMessage = string.IsNullOrWhiteSpace(guild.GreetingMessage) ? "Undefined" : guild.GreetingMessage;
                await Context.ReplyWithEmbed($"Current message: {currentMessage}");
            }

            [Command("message")]
            [Summary("Sets the greeting message to the provided message.")]
            [Priority(-10)]
            public async Task SetGreetingMessage([NotNullOrWhiteSpace] string newMessage)
            {
                var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
                guild.GreetingMessage = newMessage;
                await _context.SaveChangesAsync();
                await Context.AddConfirmation();
            }

            [Command("channel")]
            [Summary("Returns the current greeting channel.")]
            public async Task GetGreetingChannel()
            {
                var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
                var currentChannel = guild.GreetingChannel == 0 ? "Undefined" : Context.Guild.GetTextChannel(guild.GreetingChannel).Mention;
                await ReplyAsync($"Current greeting channel: {currentChannel}");
            }

            [Command("channel")]
            [Summary("Sets the greeting channel")]
            public async Task SetGreetingChannel(ITextChannel channel)
            {
                var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
                var oldChannel = guild.GreetingChannel == 0 ? "Undefined" : Context.Guild.GetTextChannel(guild.GreetingChannel).Mention;

                if (guild.GreetingChannel.Equals(channel.Id))
                {
                    await ReplyAsync($"The greeting channel was already {oldChannel}. No changes were made.");
                }
                else
                {
                    guild.GreetingChannel = channel.Id;
                    await _context.SaveChangesAsync();
                    await Context.AddConfirmation();
                }
            }

            [Command("configure", RunMode = RunMode.Async)]
            [Summary("A one-stop-shop to set up the greeting channel all in one go.")]
            public async Task ConfigureGreetingModule()
            {
                var _checkmarkEmoji = new Emoji("✅");
                var _xEmoji = new Emoji("❌");

                ITextChannel greetingChannel = null;
                string greetingMessage = null;

                Task ModifyMessage(IUserMessage modMessage, string newMessage)
                {
                    IEmbed originalEmbed = modMessage.Embeds.FirstOrDefault();
                    var newEmbed = new EmbedBuilder()
                        .WithDescription(newMessage)
                        .WithColor(originalEmbed?.Color ?? Color.Default);

                    return modMessage.ModifyAsync(x => x.Embed = newEmbed.Build());
                }

                var message = await Context.ReplyWithEmbed($"Welcome! Let's set up your guild to welcome a user.{Environment.NewLine}{Environment.NewLine}First, tell me what channel you want to use to greet people with. Either mention the channel or say \"this\" to use the current channel. Reply with \"cancel\" to cancel.");
                while (true)
                {
                    var response = await NextMessageAsync();

                    if (response is null)
                    {
                        await ModifyMessage(message, "Oops! I didn't get a response in time. If you'd like to try again, just run the command.");
                        return;
                    }

                    var responseText = response.Content;

                    if (responseText.Equals("this", StringComparison.OrdinalIgnoreCase))
                    {
                        greetingChannel = Context.Channel as ITextChannel;
                        await response.DeleteAsync();
                        break;
                    }

                    else if (responseText.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                    {
                        await ModifyMessage(message, "Operation cancelled. No changes were made.");
                        return;
                    }
                    else
                    {
                        var commandService = _services.GetRequiredService<CommandService>();
                        var typeReader = commandService.TypeReaders[typeof(SocketTextChannel)]
                            .FirstOrDefault()
                            ?? new ChannelTypeReader<SocketTextChannel>();

                        var result = await typeReader.ReadAsync(Context, responseText, _services);

                        if (result.IsSuccess)
                        {
                            greetingChannel = result.BestMatch as ITextChannel;
                            await response.DeleteAsync();
                            break;
                        }
                        else
                        {
                            await ModifyMessage(message, $"I couldn't find that channel, sorry. Try again?{Environment.NewLine}{Environment.NewLine}Tell me what channel you want to use to greet people with. Either mention the channel or say \"this\" to use the current channel. Reply with \"cancel\" to cancel.");
                            await response.DeleteAsync();
                        }
                    }
                }

                string defaultMessage = "Welcome, {UserMention}! We're glad to have you here. Please make sure to brush up on the rules and perhaps introduce yourself. Please enjoy your stay in {Guild}!";
                var sb = new StringBuilder("Great! Now we'll set up the message which I'll use to welcome new user. You can specify your own message or you can use the default.");
                sb.AppendLine();
                sb.AppendLine($"Default message: \"{defaultMessage}\"");
                sb.AppendLine();
                sb.AppendLine("You can use the following replacements:");
                sb.AppendLine("{User} ➡ The user's username");
                sb.AppendLine("{UserMention} ➡ Mention the user");
                sb.AppendLine("{Guild} ➡ The guild's name.");
                sb.AppendLine();
                sb.AppendLine("If you'd like to reference a channel, just mention it normally.");
                sb.AppendLine();
                sb.AppendLine("To use the default message, reply with \"default\". To cancel, reply with \"cancel.\" Otherwise, type a message.");
                await ModifyMessage(message, sb.ToString());
                while (true)
                {
                    var response = await NextMessageAsync(timeout: TimeSpan.FromMinutes(5));
                    var responseText = response.Content;

                    if (responseText.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        greetingMessage = defaultMessage;
                        await response.DeleteAsync();
                        break;
                    }
                    else if (responseText.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                    {
                        await ModifyMessage(message, "Operation cancelled. No changes were made.");
                        return;
                    }
                    else if (string.IsNullOrWhiteSpace(responseText))
                    {
                        sb.Length = 0;
                        sb.AppendLine("Sorry, your message can't be empty. We need to set up the message which I'll use to welcome new user. You can specify your own message or you can use the default.");
                        sb.AppendLine();
                        sb.AppendLine($"Default message: \"{defaultMessage}\"");
                        sb.AppendLine();
                        sb.AppendLine("You can use the following replacements:");
                        sb.AppendLine("{User} -> The user's username");
                        sb.AppendLine("{UserMention} -> Mention the user");
                        sb.AppendLine("{Guild} -> The guild's name.");
                        sb.AppendLine();
                        sb.AppendLine("If you'd like to reference a channel, just mention it normally.");
                        sb.AppendLine();
                        sb.AppendLine("To use the default message, reply with \"default\". To cancel, reply with \"cancel.\" Otherwise, type a message.");
                        await ModifyMessage(message, sb.ToString());
                    }
                    else
                    {
                        greetingMessage = responseText;
                        await response.DeleteAsync();
                        break;
                    }
                }

                sb.Length = 0;
                sb.AppendLine("Great, we're all set up!");
                sb.AppendLine();
                sb.AppendLine("I've not made any changes yet. I wanted you to review them before continuing. Here's what I'm going to do:");
                sb.AppendLine($"Greeting Channel: {greetingChannel.Mention}");
                sb.AppendLine($"Greeting Message: {greetingMessage}");
                sb.AppendLine();
                sb.AppendLine($"Greeting message preview: {greetingMessage.Replace("{User}", Context.User.Username).Replace("{UserMention}", Context.User.Mention).Replace("{Guild}", Context.Guild.Name)}");
                sb.AppendLine();
                sb.AppendLine("Does everything look correct?");
                await message.DeleteAsync();
                var finalResult = await Context.GetUserConfirmationAsync(sb.ToString());

                if (finalResult.Result)
                {
                    await ModifyMessage(finalResult.UserMessage, "Performing your requested changes...");

                    var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
                    guild.GreetingChannel = greetingChannel.Id;
                    guild.GreetingMessage = greetingMessage;
                    guild.GreetUser = true;

                    _context.Guilds.Update(guild);
                    await _context.SaveChangesAsync();

                    await ModifyMessage(finalResult.UserMessage, "Performing your requested changes... Done!");
                }
            }
        }

        [Name("CommandChannels")]
        [Group("commandChannels")]
        [Alias("cc")]
        [Summary("Manage the command channels")]
        [RequireBotManager]
        public class CommandChannelModule : ModuleBase
        {
            private readonly MajorContext _context;

            public CommandChannelModule(MajorContext context)
            {
                _context = context;
            }

            [Command]
            [Summary("Retrieves the current list of command channels")]
            public async Task GetCommandChannels()
            {
                var guild = await _context.Guilds.FindAsync(Context.Guild.Id);
                var commandChannels = _context.CommandChannels.Where(x => x.GuildId == Context.Guild.Id);

                async Task<string> GetCommandChannels()
                {
                    var sb = new StringBuilder();

                    foreach (var cc in commandChannels)
                    {
                        var channel = await Context.Guild.GetTextChannelAsync(cc.ChannelId);
                        sb.AppendLine(channel.Mention);
                    }

                    if (sb.Length == 0)
                        sb.Append("None");

                    return sb.ToString();
                }

                var embed = new EmbedBuilder()
                {
                    Title = "Command Channels",
                    Description = $"Use '{guild.CommandPrefix}help CommandChannel' to see more commands.",
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder()
                        {
                            IsInline = false,
                            Name = "Current Command Channels",
                            Value = GetCommandChannels()
                        }
                    },
                    Color = Color.Green
                }.WithDefaultFooter(Context.User);

                await ReplyAsync(embed: embed.Build());
            }

            [Command("add")]
            [Alias("+")]
            [Summary("Allows an operator to add a command channel.")]
            public async Task AddCommandChannel([Summary("The channel to add as a command channel")] ITextChannel channel)
            {
                await _context.CommandChannels.AddAsync(new Data.Models.CommandChannel { ChannelId = channel.Id, GuildId = Context.Channel.Id });
                await _context.SaveChangesAsync();
                await Context.AddConfirmation();
            }

            [Command("remove")]
            [Alias("-")]
            [Summary("Allows an operator to remove a command channel.")]
            public async Task RemoveCommandChannel([Summary("The channel to remove as a command channel.")] ITextChannel channel)
            {
                var cc = await _context.CommandChannels.FindAsync(channel.Id);

                if (!(cc is null))
                {
                    _context.CommandChannels.Remove(cc);
                    await _context.SaveChangesAsync();
                    await Context.AddConfirmation();
                }
                else
                {
                    await ReplyAsync("The request channel is currently not registered as a command channel. No changes were made.");
                }
            }
        }

        [Name("Modules")]
        [Group("modules")]
        [Summary("Manage the active modules")]
        [RequireBotManager]
        public class ModulesModule : ModuleBase
        {
            private readonly MajorContext _context;
            private readonly CommandService _commandService;

            public ModulesModule(MajorContext context, CommandService commandService)
            {
                _context = context;
                _commandService = commandService;
            }

            [Command]
            [Alias("list")]
            [Summary("Retrieves a list of currenty active and inactive modules")]
            [Remarks("SystemModule")]
            public async Task GetModules()
            {
                var guildModules = _commandService.Modules
                    .Select(x => x.Name.Pascalize())
                    .ToArray();
                var systemModules =
                    _commandService.Modules
                        .Where(y => y.IsSystemModule())
                        .Select(z => z.Group.Pascalize())
                        .ToArray();
                var disabledModules = _context.DisabledModules
                    .Select(x => x.ModuleName)
                    .ToArray();
                var enabledModules = guildModules
                    .Except(systemModules)
                    .Except(disabledModules);
                

                var embedBuilder = new EmbedBuilder()
                {
                    Title = "Modules",
                    Description = "System modules are always enabled.",
                    Color = Color.Green,
                    Fields =
                    {
                        new EmbedFieldBuilder()
                        {
                           IsInline = false,
                           Name = "System Modules",
                           Value = string.Join(Environment.NewLine, systemModules).OrElse(new InvalidOperationException("Could not locate any system modules. Contact the bot owner immediately."))
                        },
                        new EmbedFieldBuilder()
                        {
                            IsInline = true,
                            Name = "Enabled Modules",
                            Value = string.Join(Environment.NewLine, enabledModules).OrElse("None")
                        },
                        new EmbedFieldBuilder()
                        {
                            IsInline = true,
                            Name = "Disabled Modules",
                            Value = string.Join(Environment.NewLine, disabledModules).OrElse("None")
                        }
                    }
                }.WithDefaultFooter(Context.User);

                await ReplyAsync(embed: embedBuilder.Build());
            }

            [Command("enable")]
            [Summary("Enable the specified module")]
            public async Task EnableModule(ModuleInfo module)
            {
                if (module.IsSystemModule())
                {
                    await Context.ReplyWithEmbed($"Specified module is a system module. Nothing to do.");
                    return;
                }
                

                var foundModule = await _context.DisabledModules.FindAsync(Context.Guild.Id, module.Group);
                if (foundModule is null)
                {
                    await Context.ReplyWithEmbed("Specified module is not disabled. Nothing to do.");
                    return;
                }

                _context.DisabledModules.Remove(foundModule);
                await _context.SaveChangesAsync();
                await Context.AddConfirmation();
            }

            [Command("disable")]
            [Summary("Disables the specified module")]
            public async Task DisableModule(ModuleInfo module)
            {
                if (module.IsSystemModule())
                {
                    await Context.ReplyWithErrorEmbed("Specified module is a system module and cannot be disabled.");
                    return;
                }

                var foundModule = await _context.DisabledModules.FindAsync(Context.Guild.Id, module.Group);
                if(!(foundModule is null))
                {
                    await Context.ReplyWithEmbed("Specified module is already disabled. Nothing to do.");
                    return;
                }

                await _context.DisabledModules.AddAsync(new Data.Models.Module() { GuildId = Context.Guild.Id, ModuleName = module.Group });
                await _context.SaveChangesAsync();
                await Context.AddConfirmation();
            }
        }
    }
}
