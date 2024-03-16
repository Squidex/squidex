// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class CommandResultTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var sut = new CommandResult(DomainId.NewGuid(), 3, 2, "result");

            var serialized = sut.SerializeAndDeserialize();

            Assert.Equal(sut, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_empty()
        {
            var sut = CommandResult.Empty(DomainId.NewGuid(), 3, 2);

            var serialized = sut.SerializeAndDeserialize();

            Assert.Equal(sut, serialized);
        }
    }
}
