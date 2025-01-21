// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.MongoDb.TestHelpers;

namespace Squidex.MongoDb.Domain.Contents;

public sealed class StatusSerializerTests
{
    [Fact]
    public void Should_serialize_and_deserialize_status()
    {
        var source = Status.Published;

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_default_status()
    {
        var source = default(Status);

        var deserialized = source.SerializeAndDeserializeBson();

        Assert.Equal(source, deserialized);
    }
}
