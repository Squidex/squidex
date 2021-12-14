// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.States
{
    public class InconsistentStateExceptionTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var source = new InconsistentStateException(100, 200, new InvalidOperationException("Inner"));
            var result = source.SerializeAndDeserializeBinary();

            Assert.IsType<InvalidOperationException>(result.InnerException);

            Assert.Equal("Inner", result.InnerException?.Message);

            Assert.Equal(result.VersionExpected, source.VersionExpected);
            Assert.Equal(result.VersionCurrent, source.VersionCurrent);

            Assert.Equal(result.Message, source.Message);
        }
    }
}
