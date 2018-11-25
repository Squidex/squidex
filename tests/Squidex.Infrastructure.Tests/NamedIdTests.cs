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

            var token1a = NamedId.Of(id1, "my-name1");
            var token1b = NamedId.Of(id1, "my-name1");
            var token1c = NamedId.Of(id1, "my-name2");
            var token2a = NamedId.Of(id2, "my-name1");

            Assert.True(token1a.Equals(token1b));

            Assert.False(token1a.Equals(token2a));
            Assert.False(token1a.Equals(token1c));
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            object token1a = NamedId.Of(id1, "my-name1");
            object token1b = NamedId.Of(id1, "my-name1");
            object token1c = NamedId.Of(id1, "my-name2");
            object token2a = NamedId.Of(id2, "my-name1");

            Assert.True(token1a.Equals(token1b));

            Assert.False(token1a.Equals(token2a));
            Assert.False(token1a.Equals(token1c));
        }

        [Fact]
        public void Should_provide_correct_hash_codes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            object token1a = NamedId.Of(id1, "my-name1");
            object token1b = NamedId.Of(id1, "my-name1");
            object token1c = NamedId.Of(id1, "my-name2");
            object token2a = NamedId.Of(id2, "my-name1");

            Assert.Equal(token1a.GetHashCode(), token1b.GetHashCode());

            Assert.NotEqual(token1a.GetHashCode(), token2a.GetHashCode());
            Assert.NotEqual(token1a.GetHashCode(), token1c.GetHashCode());
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
