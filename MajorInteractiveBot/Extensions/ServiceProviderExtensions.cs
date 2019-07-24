using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MajorInteractiveBot.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredServiceOrThrow<T>(this IServiceProvider provider)
        {
            T obj = provider.GetRequiredService<T>();
            if (null == obj)
                throw new InvalidOperationException($"Instance of type {nameof(T)} does not exist in {nameof(provider)}");

            return obj;
        }
    }
}
