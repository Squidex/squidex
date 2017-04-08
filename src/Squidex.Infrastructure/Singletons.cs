// ==========================================================================
//  Singletons.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;

namespace Squidex.Infrastructure
{
    public static class Singletons<T>
    {
        private static readonly ConcurrentDictionary<string, T> instances = new ConcurrentDictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        public static T GetOrAdd(string key, Func<string, T> factory)
        {
            return instances.GetOrAdd(key, factory);
        }
    }
}
