using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Extensions
{
    public static class CommandContextExtensions
    {
        private static readonly Emoji _checkmarkEmoji = new Emoji("✅");
        private static readonly Emoji _warnEmoji = new Emoji("⚠");
        private static readonly Emoji _xEmoji = new Emoji("❌");
        private const int _confirmationTimeoutSeconds = 10;

        public static Task AddConfirmation(this ICommandContext context)
            => context.Message.AddReactionAsync(_checkmarkEmoji);

        public static Task AddWarning(this ICommandContext context)
            => context.Message.AddReactionAsync(_warnEmoji);

        public static Task AddError(this ICommandContext context)
            => context.Message.AddReactionAsync(_xEmoji);

        public static async Task<OperationResult<bool>> GetUserConfirmationAsync(this ICommandContext context, string mainMessage)
        {
            if (!mainMessage.EndsWith(Environment.NewLine))
                mainMessage += Environment.NewLine;

            var confirmationMessage = await ReplyWithEmbed(context, mainMessage +
                $"React with {_checkmarkEmoji} or {_xEmoji} in the next {_confirmationTimeoutSeconds} seconds to finalize or cancel the operation.");

            await confirmationMessage.AddReactionAsync(_checkmarkEmoji);
            await confirmationMessage.AddReactionAsync(_xEmoji);

            for (var i = 0; i < _confirmationTimeoutSeconds; i++)
            {
                await Task.Delay(1000);

                var denyingUsers = await confirmationMessage.GetReactionUsersAsync(_xEmoji, int.MaxValue).FlattenAsync();
                if (denyingUsers.Any(u => u.Id == context.User.Id))
                {
                    await RemoveReactionsAndUpdateMessage("Cancellation was successfully received. Cancelling the operation.");
                    return new OperationResult<bool>(false, confirmationMessage);
                }

                var confirmingUsers = await confirmationMessage.GetReactionUsersAsync(_checkmarkEmoji, int.MaxValue).FlattenAsync();
                if (confirmingUsers.Any(u => u.Id == context.User.Id))
                {
                    await RemoveReactionsAndUpdateMessage("Confirmation was successfully received. Performing the operation.");
                    return new OperationResult<bool>(true, confirmationMessage);
                }
            }

            await RemoveReactionsAndUpdateMessage("Confirmation was not received. Cancelling the operation.");
            return new OperationResult<bool>(false, confirmationMessage);

            async Task RemoveReactionsAndUpdateMessage(string bottomMessage)
            {
                var newEmbed = new EmbedBuilder().WithDescription(mainMessage + bottomMessage);
                await confirmationMessage.RemoveAllReactionsAsync();
                // await confirmationMessage.ModifyAsync(m => m.Content = mainMessage + bottomMessage);
                await confirmationMessage.ModifyAsync(m => m.Embed = newEmbed.WithColor(m.Embed.IsSpecified ? m.Embed.Value.Color.Value : Color.Default).Build());
            }
        }

        public static Task<IUserMessage> ReplyWithEmbed(this ICommandContext context, string message)
            => EmbedSend(context, message, Color.Green);
        public static Task<IUserMessage> ReplyWithErrorEmbed(this ICommandContext context, string message)
            => EmbedSend(context, message, Color.Red);

        private static Task<IUserMessage> EmbedSend(ICommandContext context, string message, Color color, bool useDefaultFooter = true)
        {
            var embed = new EmbedBuilder()
            {
                Description = message,
                Color = color
            };

            if (useDefaultFooter)
            {
                embed.WithDefaultFooter(context.User);
            }

            return context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }

    public class OperationResult<T>
    {
        public T Result { get; }
        public IUserMessage UserMessage { get; }

        public OperationResult(T result, IUserMessage userMessage)
        {
            Result = result;
            UserMessage = userMessage;
        }
    }
}
