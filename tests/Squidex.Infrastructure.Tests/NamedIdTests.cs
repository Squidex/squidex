// ==========================================================================
//  NamedIdTest.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Json;
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

            var namedId = new NamedId<Guid>(id, "my-name");

            Assert.Equal(id, namedId.Id);
            Assert.Equal("my-name", namedId.Name);
        }

        [Fact]
        public void Should_convert_named_id_to_string()
        {
            var id = Guid.NewGuid();

            var namedId = new NamedId<Guid>(id, "my-name");

            Assert.Equal($"{id},my-name", namedId.ToString());
        }

        [Fact]
        public void Should_make_correct_equal_comparisons()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var token1a = new NamedId<Guid>(id1, "my-name1");
            var token1b = new NamedId<Guid>(id1, "my-name1");
            var token1c = new NamedId<Guid>(id1, "my-name2");
            var token2a = new NamedId<Guid>(id2, "my-name1");

            Assert.True(token1a.Equals(token1b));

            Assert.False(token1a.Equals(token2a));
            Assert.False(token1a.Equals(token1c));
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            object token1a = new NamedId<Guid>(id1, "my-name1");
            object token1b = new NamedId<Guid>(id1, "my-name1");
            object token1c = new NamedId<Guid>(id1, "my-name2");
            object token2a = new NamedId<Guid>(id2, "my-name1");

            Assert.True(token1a.Equals(token1b));

            Assert.False(token1a.Equals(token2a));
            Assert.False(token1a.Equals(token1c));
        }

        [Fact]
        public void Should_provide_correct_hash_codes()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            object token1a = new NamedId<Guid>(id1, "my-name1");
            object token1b = new NamedId<Guid>(id1, "my-name1");
            object token1c = new NamedId<Guid>(id1, "my-name2");
            object token2a = new NamedId<Guid>(id2, "my-name1");

            Assert.Equal(token1a.GetHashCode(), token1b.GetHashCode());

            Assert.NotEqual(token1a.GetHashCode(), token2a.GetHashCode());
            Assert.NotEqual(token1a.GetHashCode(), token1c.GetHashCode());
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_guid_token()
        {
            JsonHelper.SerializeAndDeserialize<NamedId<Guid>>(null, new NamedGuidIdConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_guid_token()
        {
            new NamedId<Guid>(Guid.NewGuid(), "my-name").SerializeAndDeserialize(new NamedGuidIdConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_long_token()
        {
            JsonHelper.SerializeAndDeserialize<NamedId<long>>(null, new NamedLongIdConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_long_token()
        {
            new NamedId<long>(123, "my-name").SerializeAndDeserialize(new NamedLongIdConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_string_token()
        {
            JsonHelper.SerializeAndDeserialize<NamedId<string>>(null, new NamedStringIdConverter());
        }

        [Fact]
        public void Should_serialize_and_deserialize_valid_string_token()
        {
            new NamedId<string>(Guid.NewGuid().ToString(), "my-name").SerializeAndDeserialize(new NamedStringIdConverter());
        }

        [Fact]
        public void Should_throw_exception_if_string_id_is_not_valid()
        {
            JsonHelper.DoesNotDeserialize<NamedId<string>>("123", new NamedStringIdConverter());
        }

        [Fact]
        public void Should_throw_exception_if_long_id_is_not_valid()
        {
            JsonHelper.DoesNotDeserialize<NamedId<long>>("123", new NamedLongIdConverter());
            JsonHelper.DoesNotDeserialize<NamedId<long>>("invalid-long,name", new NamedLongIdConverter());
        }

        [Fact]
        public void Should_throw_exception_if_guid_id_is_not_valid()
        {
            JsonHelper.DoesNotDeserialize<NamedId<Guid>>("123", new NamedGuidIdConverter());
            JsonHelper.DoesNotDeserialize<NamedId<Guid>>("invalid-guid,name", new NamedGuidIdConverter());
        }
    }
}
