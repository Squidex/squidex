// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;

namespace TestSuite;

public static class Factories
{
    private static readonly ConcurrentDictionary<string, Task<object>> Instances = new ConcurrentDictionary<string, Task<object>>();

    public static async Task<T> CreateAsync<T>(string key, Func<Task<T>> factory)
    {
        return (T)await Instances.GetOrAdd(key, async (_, f) => await f(), factory);
    }
}
