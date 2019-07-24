using System;

namespace MajorInteractiveBot.Attributes
{
    /// <summary>
    /// Hides the module or command from display
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class HiddenFromHelpAttribute : Attribute
    {
    }
}
