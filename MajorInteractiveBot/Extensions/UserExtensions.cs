using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace MajorInteractiveBot.Extensions
{
    public static class UserExtensions
    {
        public static string UsernameAndDiscrim(this IUser user)
            => $"{user.Username}#{user.Discriminator}";
    }
}
