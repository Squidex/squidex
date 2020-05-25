// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure
{
    public class DomainIdTests
    {
        private readonly TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(DomainId));

        [Fact]
        public void Should_convert_from_string()
        {
            var result = typeConverter.ConvertFromString("123");

            Assert.Equal(new DomainId("123"), result);
        }

        [Fact]
        public void Should_convert_from_guid()
        {
            var result = typeConverter.ConvertFrom(Guid.Empty);

            Assert.Equal(new DomainId(Guid.Empty), result);
        }

        [Fact]
        public void Should_convert_to_string()
        {
            var result = typeConverter.ConvertToString(new DomainId("123"));

            Assert.Equal("123", result);
        }

        [Fact]
        public void Should_initialize_default()
        {
            DomainId domainId = default;

            Assert.Equal("<EMPTY>", domainId.ToString());
        }

        [Fact]
        public void Should_initialize_domainId_from_guid()
        {
            var domainId = new DomainId(Guid.Empty);

            Assert.Equal(Guid.Empty.ToString(), domainId.ToString());
        }

        [Fact]
        public void Should_initialize_domainId_from_string()
        {
            var domainId = new DomainId("Custom");

            Assert.Equal("Custom", domainId.ToString());
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var domainId_1_a = new DomainId("1");
            var domainId_1_b = new DomainId("1");

            var domainId2_a = new DomainId("2");

            Assert.Equal(domainId_1_a, domainId_1_b);
            Assert.Equal(domainId_1_a.GetHashCode(), domainId_1_b.GetHashCode());
            Assert.True(domainId_1_a.Equals((object)domainId_1_b));

            Assert.NotEqual(domainId_1_a, domainId2_a);
            Assert.NotEqual(domainId_1_a.GetHashCode(), domainId2_a.GetHashCode());
            Assert.False(domainId_1_a.Equals((object)domainId2_a));

            Assert.True(domainId_1_a == domainId_1_b);
            Assert.True(domainId_1_a != domainId2_a);

            Assert.False(domainId_1_a != domainId_1_b);
            Assert.False(domainId_1_a == domainId2_a);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var domainId = new DomainId("123");

            var serialized = domainId.SerializeAndDeserialize();

            Assert.Equal(domainId, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_as_dictionary()
        {
            var dictionary = new Dictionary<DomainId, int>
            {
                [new DomainId("123")] = 321
            };

            var serialized = dictionary.SerializeAndDeserialize();

            Assert.Equal(321, serialized[new DomainId("123")]);
        }
    }
}
