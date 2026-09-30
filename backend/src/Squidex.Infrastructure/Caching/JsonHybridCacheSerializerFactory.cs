// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.Caching;

public sealed class JsonHybridCacheSerializerFactory(IJsonSerializer serializer) : IHybridCacheSerializerFactory
{
    public bool TryCreateSerializer<T>([NotNullWhen(true)] out IHybridCacheSerializer<T>? result)
    {
        result = new JsonHybridCacheSerializer<T>(serializer);
        return true;
    }
}
