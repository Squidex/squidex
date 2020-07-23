﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure
{
    public class DomainObjectExceptionTests
    {
        [Fact]
        public void Should_serialize_and_deserialize_DomainObjectDeletedException()
        {
            var source = new DomainObjectDeletedException("123");
            var result = source.SerializeAndDeserializeBinary();

            Assert.Equal(result.Id, source.Id);

            Assert.Equal(result.Message, source.Message);
        }

        [Fact]
        public void Should_serialize_and_deserialize_DomainObjectNotFoundException()
        {
            var source = new DomainObjectNotFoundException("123");
            var result = source.SerializeAndDeserializeBinary();

            Assert.Equal(result.Id, source.Id);

            Assert.Equal(result.Message, source.Message);
        }

        [Fact]
        public void Should_serialize_and_deserialize_DomainObjectVersionExceptionn()
        {
            var source = new DomainObjectVersionException("123", 100, 200);
            var result = source.SerializeAndDeserializeBinary();

            Assert.Equal(result.Id, source.Id);
            Assert.Equal(result.ExpectedVersion, source.ExpectedVersion);
            Assert.Equal(result.CurrentVersion, source.CurrentVersion);

            Assert.Equal(result.Message, source.Message);
        }
    }
}
