// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure;

public class DomainObjectExceptionTests
{
    [Fact]
    public void Should_serialize_and_deserialize_DomainException()
    {
        var source = new DomainException("Message", "ErrorCode");
        var actual = source.SerializeAndDeserializeBinary();

        Assert.Equal(actual.ErrorCode, source.ErrorCode);
        Assert.Equal(actual.Message, source.Message);
    }

    [Fact]
    public void Should_serialize_and_deserialize_DomainObjectDeletedException()
    {
        var source = new DomainObjectDeletedException("123");
        var actual = source.SerializeAndDeserializeBinary();

        Assert.Equal(actual.Id, source.Id);
        Assert.Equal(actual.Message, source.Message);
    }

    [Fact]
    public void Should_serialize_and_deserialize_DomainObjectNotFoundException()
    {
        var source = new DomainObjectNotFoundException("123");
        var actual = source.SerializeAndDeserializeBinary();

        Assert.Equal(actual.Id, source.Id);
        Assert.Equal(actual.Message, source.Message);
    }

    [Fact]
    public void Should_serialize_and_deserialize_DomainObjectVersionExceptionn()
    {
        var source = new DomainObjectVersionException("123", 100, 200);
        var actual = source.SerializeAndDeserializeBinary();

        Assert.Equal(actual.Id, source.Id);
        Assert.Equal(actual.ExpectedVersion, source.ExpectedVersion);
        Assert.Equal(actual.CurrentVersion, source.CurrentVersion);
        Assert.Equal(actual.Message, source.Message);
    }
}
