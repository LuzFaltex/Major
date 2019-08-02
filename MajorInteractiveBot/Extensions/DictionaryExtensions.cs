using System;
using System.Collections.Generic;
using System.Text;

namespace MajorInteractiveBot.Extensions
{
    public static class DictionaryExtensions
    {
        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<TKey, TValue> action)
        {
            foreach (var kvp in dictionary)
                action(kvp.Key, kvp.Value);
        }
    }
}
