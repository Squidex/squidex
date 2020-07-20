// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Caching
{
    public interface IReplicatedCache
    {
        void Add(string key, object? value, TimeSpan expiration, bool invalidate);

        void Remove(string key);

        bool TryGetValue(string key, out object? value);
    }
}
