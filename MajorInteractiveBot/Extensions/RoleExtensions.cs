using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace MajorInteractiveBot.Extensions
{
    public static class RoleExtensions
    {
        public static string NameAndId(this IRole role)
            => $"{role.Name} ({role.Id})";
    }
}
