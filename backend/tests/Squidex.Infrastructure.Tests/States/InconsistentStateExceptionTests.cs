// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.States;

public class InconsistentStateExceptionTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var source = new InconsistentStateException(100, 200, new InvalidOperationException("Inner"));
        var actual = source.SerializeAndDeserializeBinary();

        Assert.IsType<InvalidOperationException>(actual.InnerException);

        Assert.Equal("Inner", actual.InnerException?.Message);

        Assert.Equal(actual.VersionExpected, source.VersionExpected);
        Assert.Equal(actual.VersionCurrent, source.VersionCurrent);

        Assert.Equal(actual.Message, source.Message);
    }
}
