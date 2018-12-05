// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure
{
    public class NamedIdTests
    {
        [Fact]
        public void Should_instantiate_token()
        {
            var id = Guid.NewGuid();

            var namedId = NamedId.Of(id, "my-name");

            Assert.Equal(id, namedId.Id);
            Assert.Equal("my-name", namedId.Name);
        }

        [Fact]
        public void Should_convert_named_id_to_string()
        {
            var id = Guid.NewGuid();

            var namedId = NamedId.Of(id, "my-name");

            Assert.Equal($"{id},my-name", namedId.ToString());
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var named_id1_name1_a = NamedId.Of(id1, "name1");
            var named_id1_name1_b = NamedId.Of(id1, "name1");

            var named_id2_name1 = NamedId.Of(id2, "name1");
            var named_id1_name2 = NamedId.Of(id1, "name2");

            Assert.Equal(named_id1_name1_a, named_id1_name1_b);
            Assert.Equal(named_id1_name1_a.GetHashCode(), named_id1_name1_b.GetHashCode());
            Assert.True(named_id1_name1_a.Equals((object)named_id1_name1_b));

            Assert.NotEqual(named_id1_name1_a, named_id2_name1);
            Assert.NotEqual(named_id1_name1_a.GetHashCode(), named_id2_name1.GetHashCode());
            Assert.False(named_id1_name1_a.Equals((object)named_id2_name1));

            Assert.NotEqual(named_id1_name1_a, named_id1_name2);
            Assert.NotEqual(named_id1_name1_a.GetHashCode(), named_id1_name2.GetHashCode());
            Assert.False(named_id1_name1_a.Equals((object)named_id1_name2));
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_guid_token()
        {
            NamedId<Guid> value = null;

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_guid_token()
        {
            var value = NamedId.Of(Guid.NewGuid(), "my-name");

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_long_token()
        {
            NamedId<long> value = null;

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_long_token()
        {
            var value = NamedId.Of(123L, "my-name");

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_string_token()
        {
            NamedId<string> value = null;

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_string_token()
        {
            var value = NamedId.Of(Guid.NewGuid().ToString(), "my-name");

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_throw_exception_if_string_id_is_not_valid()
        {
            Assert.ThrowsAny<Exception>(() => JsonHelper.Deserialize<NamedId<string>>("123"));
        }

        [Fact]
        public void Should_throw_exception_if_long_id_is_not_valid()
        {
            Assert.ThrowsAny<Exception>(() => JsonHelper.Deserialize<NamedId<long>>("invalid-long,name"));
        }

        [Fact]
        public void Should_throw_exception_if_guid_id_is_not_valid()
        {
            Assert.ThrowsAny<Exception>(() => JsonHelper.Deserialize<NamedId<Guid>>("invalid-guid,name"));
        }
    }
}
