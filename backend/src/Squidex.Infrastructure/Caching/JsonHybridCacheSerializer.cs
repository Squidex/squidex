// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Buffers;
using Microsoft.Extensions.Caching.Hybrid;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.Caching;

public sealed class JsonHybridCacheSerializer<T>(IJsonSerializer serializer) : IHybridCacheSerializer<T>
{
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        using var stream = new MemoryStream(source.ToArray(), false);

        return serializer.Deserialize<T>(stream);
    }

    public void Serialize(T value, IBufferWriter<byte> target)
    {
        target.Write(serializer.SerializeToBytes(value));
    }
}
