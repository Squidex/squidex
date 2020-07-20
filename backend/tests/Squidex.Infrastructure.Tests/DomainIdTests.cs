﻿// ==========================================================================
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
        public void Should_initialize_default()
        {
            DomainId domainId = default;

            Assert.Equal(Guid.Empty.ToString(), domainId.ToString());
        }

        [Fact]
        public void Should_initialize_default_from_string()
        {
            var domainId = DomainId.Create(Guid.Empty.ToString());

            Assert.Equal(DomainId.Empty, domainId);
        }

        [Fact]
        public void Should_create_nullable_from_string()
        {
            var domainId = DomainId.CreateNullable(null);

            Assert.Null(domainId);
        }

        [Fact]
        public void Should_convert_from_string()
        {
            var text = "123";

            var result = typeConverter.ConvertFromString(text);

            Assert.Equal(DomainId.Create(text), result);
        }

        [Fact]
        public void Should_convert_from_guid()
        {
            var guid = Guid.NewGuid();

            var result = typeConverter.ConvertFrom(guid);

            Assert.Equal(guid.ToString(), result.ToString());
        }

        [Fact]
        public void Should_convert_to_string()
        {
            var text = "123";

            var result = typeConverter.ConvertToString(DomainId.Create(text));

            Assert.Equal(text, result);
        }

        [Fact]
        public void Should_initialize_domainId_from_guid()
        {
            var guid = Guid.NewGuid();

            var domainId = DomainId.Create(guid);

            Assert.Equal(guid.ToString(), domainId.ToString());
        }

        [Fact]
        public void Should_initialize_domainId_from_string()
        {
            var text = "Custom";

            var domainId = DomainId.Create(text);

            Assert.Equal(text, domainId.ToString());
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var domainId_1_a = DomainId.Create("1");
            var domainId_1_b = DomainId.Create("1");

            var domainId2_a = DomainId.Create("2");

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
            var domainId = DomainId.Create("123");

            var serialized = domainId.SerializeAndDeserialize();

            Assert.Equal(domainId, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_as_dictionary()
        {
            var dictionary = new Dictionary<DomainId, int>
            {
                [DomainId.Create("123")] = 321
            };

            var serialized = dictionary.SerializeAndDeserialize();

            Assert.Equal(321, serialized[DomainId.Create("123")]);
        }
    }
}
