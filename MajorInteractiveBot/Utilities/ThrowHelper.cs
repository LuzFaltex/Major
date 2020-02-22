using System;

namespace MajorInteractiveBot.Utilities
{
    internal static class ThrowHelper
    {
        internal static void ThrowNotImplementedException()
            => throw new NotImplementedException();

        internal static void ThrowInvalidOperationException(string message)
            => throw new InvalidOperationException(message);

        internal static void ThrowNotNullOrEmptyException(string paramName)
            => throw new ArgumentException(ExceptionMessages.NotNullOrEmpty, paramName);
    }

    internal static class ExceptionMessages
    {
        internal const string NotNullOrEmpty = "Provided value must not be null or empty.";
    }
}
