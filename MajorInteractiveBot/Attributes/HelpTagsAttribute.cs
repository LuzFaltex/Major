using System;
using System.Collections.Generic;
using System.Text;

namespace MajorInteractiveBot.Attributes
{
    /// <summary>
    /// Indicates tags to use during help searches to increase the hit rate of the module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HelpTagsAttribute : Attribute
    {
        public string[] Tags { get; }
        public HelpTagsAttribute(params string[] tags)
        {
            Tags = tags;
        }
    }
}
