using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace MajorInteractiveBot.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class NotNullOrEmptyAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (!(value is string sVal))
                return Task.FromResult(PreconditionResult.FromError("Value provided must be a string."));

            if (string.IsNullOrEmpty(sVal))
                return Task.FromResult(PreconditionResult.FromError("Value must be provided"));

            else return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
