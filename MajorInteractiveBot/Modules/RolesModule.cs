using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MajorInteractiveBot.Attributes;
using MajorInteractiveBot.Data;
using MajorInteractiveBot.Data.Models;
using MajorInteractiveBot.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Modules
{
    [Name("Roles")]
    [Group("roles")]
    [Alias("role")]
    [Summary("Allows a user to self-assign roles.")]
    [Module]
    public class RolesModule : InteractiveBase
    {
        private readonly MajorContext _dbContext;
        private readonly ILogger Log;
        private const string CodeBlock = @"```
{0}
```";

        private readonly ReactionList AllReactions = new ReactionList()
        {
            Backward = true,
            First = true,
            Forward = true,
            Info = true,
            Jump = true,
            Last = true,
            Trash = true
        };

        public RolesModule(MajorContext dbContext, ILogger<RolesModule> logger)
        {
            _dbContext = dbContext;
            Log = logger;
        }

        [Command]
        [Alias("list")]
        [Summary("Provides a listing of the roles a user can subscribe to.")]
        public async Task ListRoles()
        {
            Dictionary<ulong, List<AssignableRole>> roleMappings = new Dictionary<ulong, List<AssignableRole>>();
            var roles = _dbContext.AssignableRoles
                .Where(x => x.GuildId == Context.Guild.Id)
                .ToArray();

            if (roles.Count() == 0)
            {
                await Context.ReplyWithEmbed("No available roles.");
                return;
            }

            foreach(var role in roles)
            {
                if (!roleMappings.TryGetValue(role.RoleCategory, out var list))
                    list = new List<AssignableRole>();
                list.Add(role);
                list.OrderByDescending(x => x.Position);
                roleMappings[role.RoleCategory] = list;
            }            

            List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();

            if (roleMappings.TryGetValue(Context.Guild.EveryoneRole.Id, out var generalCategory))
            {
                roleMappings.Remove(Context.Guild.EveryoneRole.Id);

                pages.Add(BuildPage(Context.Guild.EveryoneRole.Id, generalCategory));
            }

            var orderedRoleMappings = roleMappings
                    .OrderByDescending(x =>
                        Context.Guild.GetRole(x.Key).Position);

            foreach(var value in orderedRoleMappings)
            {
                pages.Add(BuildPage(value.Key, value.Value));
            }

            var guild = await _dbContext.Guilds.FindAsync(Context.Guild.Id);
            var pager = new PaginatedMessage()
            {
                Title = "Assignable Roles",
                Description = "Click the information button for more info",
                Pages = pages,
                Color = Color.Green,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = new StringBuilder()
                    .AppendLine($"**Add Role:** {guild.CommandPrefix}roles add <role>")
                    .AppendLine($"**Remove Role:** {guild.CommandPrefix}roles remove <role>")
                    .AppendLine()
                    .AppendLine("Hint: `add` and `remove` can be swapped for `+` and `-`, respectively.")
                    .ToString(),
                    Timeout = TimeSpan.FromMinutes(5)
                }
            };

            await PagedReplyAsync(pager, AllReactions, false);
            

            PaginatedMessage.Page BuildPage(ulong category, List<AssignableRole> aroles)
            {
                var roleNames = new List<string>();
                foreach (var role in aroles)
                {
                    var roleName = Context.Guild.GetRole(role.RoleId).Name;
                    roleName += role.Require18Plus ? "*" : "";
                    roleNames.Add(roleName);
                }

                var categoryRole = Context.Guild.GetRole(category);
                var page = new PaginatedMessage.Page()
                {
                    Title = "Category: " + categoryRole.Name.Replace("=", "").Trim(),
                    Description = "Below are a list of roles you can assign yourself. Roles marked with a * require the 18+ role." + Environment.NewLine + string.Format(CodeBlock, string.Join(Environment.NewLine, roleNames))
                };

                return page;
            }
        }

        [Command("add")]
        [Alias("+")]
        [Summary("Add a role to yourself.")]
        [RequireContext(ContextType.Guild)]
        public async Task AddRole([Remainder] IRole role)
        {
            // first let's check if the role is assignable.
            if ((await _dbContext.AssignableRoles.FindAsync(role.Id)) is AssignableRole arole)
            {
                var guild = await _dbContext.Guilds.FindAsync(Context.Guild.Id);
                var guildUser = Context.User as SocketGuildUser;
                var categoryRole = Context.Guild.GetRole(arole.RoleCategory);
                if (arole.Require18Plus)
                {
                    if (guild.AdultRole == 0)
                    {
                        await Context.ReplyWithErrorEmbed("This role requires the 18+ role, but no 18+ role is configured. Please contact the server admin.");
                    }

                    IRole adultRole;

                    try
                    {
                        adultRole = Context.Guild.GetRole(guild.AdultRole);
                    }
                    catch(Exception ex)
                    {
                        Log.LogCritical(ex, "The 18+ role is misconfigured.");
                        throw;
                    }

                    if (guildUser.HasRole(adultRole))
                    {
                        await guildUser.AddRoleAsync(role);
                    }
                    else
                    {
                        await Context.ReplyWithErrorEmbed("You must have the 18+ role to receive this role. Please talk to a staff member about the verification process.");
                    }
                }
                else
                {
                    await guildUser.AddRoleAsync(role);                    
                }

                if (!guildUser.HasRole(categoryRole))
                {
                    await guildUser.AddRoleAsync(categoryRole);
                }
                await Context.AddConfirmation();
            }
            else
            {
                await Context.AddWarning();
            }
        }

        [Command("remove")]
        [Alias("-")]
        [Summary("Removes a role from yourself.")]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveRole([Remainder] IRole role)
        {
            // first let's check if the role is assignable.
            if ((await _dbContext.AssignableRoles.FindAsync(role.Id)) is AssignableRole arole)
            {
                var guildUser = Context.User as SocketGuildUser;

                if (guildUser.HasRole(role))
                {
                    await guildUser.RemoveRoleAsync(role);
                    await Context.AddConfirmation();
                }
                else
                {
                    await Context.AddWarning();
                }
            }
            else
            {
                await Context.AddWarning();
            }
        }

        [Command("register")]
        [Summary("Make a role assignable")]
        [RequireBotManager]
        public async Task RegisterRole(IRole role, IRole category, bool requireAdult = false)
        {
            var arole = new AssignableRole()
            {
                GuildId = Context.Guild.Id,
                Position = role.Position,
                Require18Plus = requireAdult,
                RoleCategory = category.Id,
                RoleId = role.Id
            };

            await _dbContext.AssignableRoles.AddAsync(arole);
            await _dbContext.SaveChangesAsync();
            await Context.AddConfirmation();
        }

        [Command("unregister")]
        [Summary("Make a role unassignable")]
        [RequireBotManager]
        public async Task UnregisterRole(IRole role)
        {
            var arole = await _dbContext.AssignableRoles.FindAsync(role.Id);
            if (arole is null)
            {
                await Context.AddWarning();
                return;
            }

            _dbContext.AssignableRoles.Remove(arole);
            await _dbContext.SaveChangesAsync();
            await Context.AddConfirmation();
        }
    }
}
