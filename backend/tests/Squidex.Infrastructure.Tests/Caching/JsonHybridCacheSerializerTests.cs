// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Buffers;
using Microsoft.Extensions.Caching.Hybrid;
using Squidex.Infrastructure.TestHelpers;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Caching;

public class JsonHybridCacheSerializerTests
{
    public sealed record Value(string Text, int Number, DomainId Id);

    [Fact]
    public void Should_serialize_and_deserialize_value()
    {
        var source = new Value("text", 42, DomainId.NewGuid());

        var actual = SerializeAndDeserialize(new JsonHybridCacheSerializer<Value>(TestUtils.DefaultSerializer), source);

        Assert.Equal(source, actual);
    }

    [Fact]
    public void Should_create_serializer_for_any_type()
    {
        var factory = new JsonHybridCacheSerializerFactory(TestUtils.DefaultSerializer);

        Assert.True(factory.TryCreateSerializer<Value>(out var serializer));

        var source = new Value("text", 42, DomainId.NewGuid());

        Assert.Equal(source, SerializeAndDeserialize(serializer, source));
    }

    private static T SerializeAndDeserialize<T>(IHybridCacheSerializer<T> serializer, T value)
    {
        var buffer = new ArrayBufferWriter<byte>();

        serializer.Serialize(value, buffer);

        return serializer.Deserialize(new ReadOnlySequence<byte>(buffer.WrittenMemory));
    }
}
