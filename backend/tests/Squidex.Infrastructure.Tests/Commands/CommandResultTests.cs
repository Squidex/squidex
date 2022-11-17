// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.Commands;

public class CommandResultTests
{
    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var sut = new CommandResult(DomainId.NewGuid(), 3, 2, null!);

        var serialized = sut.SerializeAndDeserialize();

        Assert.Equal(sut, serialized);
    }
}
