using Discord;
using Discord.WebSocket;
using System.Linq;

namespace MajorInteractiveBot.Extensions
{
    public static class UserExtensions
    {
        public static string UsernameAndDiscrim(this IUser user)
            => $"{user.Username}#{user.Discriminator}";

        public static string GetAvatarUrlOrDefault(this IUser user)
            => user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();

        public static bool HasRole(this IGuildUser user, IRole role)
            => user.RoleIds.Contains(role.Id);            
    }
}
