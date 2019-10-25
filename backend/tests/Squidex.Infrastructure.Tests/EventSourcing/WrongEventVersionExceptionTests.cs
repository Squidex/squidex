// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class WrongEventVersionExceptionTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var source = new WrongEventVersionException(100, 200);
            var result = source.SerializeAndDeserializeBinary();

            Assert.Equal(result.ExpectedVersion, source.ExpectedVersion);
            Assert.Equal(result.CurrentVersion, source.CurrentVersion);

            Assert.Equal(result.Message, source.Message);
        }
    }
}
