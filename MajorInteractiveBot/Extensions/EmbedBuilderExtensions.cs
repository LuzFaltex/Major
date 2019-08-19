using Discord;

namespace MajorInteractiveBot.Extensions
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder WithDefaultFooter(this EmbedBuilder builder, IUser user)
        {
            builder.Footer = new EmbedFooterBuilder()
            {
                IconUrl = user.GetAvatarUrlOrDefault(),
                Text = $"Requested by {user.UsernameAndDiscrim()}"
            };
            return builder;
        }
    }
}
