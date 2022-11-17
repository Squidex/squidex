// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing;

public class WrongEventVersionExceptionTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var source = new WrongEventVersionException(100, 200);
        var actual = source.SerializeAndDeserializeBinary();

        Assert.Equal(actual.ExpectedVersion, source.ExpectedVersion);
        Assert.Equal(actual.CurrentVersion, source.CurrentVersion);

        Assert.Equal(actual.Message, source.Message);
    }
}
