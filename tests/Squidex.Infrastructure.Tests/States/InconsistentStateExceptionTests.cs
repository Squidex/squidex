// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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

            Assert.Equal(result.ExpectedVersion, source.ExpectedVersion);
            Assert.Equal(result.CurrentVersion, source.CurrentVersion);

            Assert.Equal(result.Message, source.Message);
        }
    }
}
