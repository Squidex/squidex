// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Json.Objects
{
    public class JsonValuesSerializationTests
    {
        [Fact]
        public void Should_deserialize_integer()
        {
            var serialized = TestUtils.Deserialize<JsonValue>(123);

            Assert.Equal(JsonValue.Create(123), serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_null()
        {
            var value = JsonValue.Null;

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_date()
        {
            var value = JsonValue.Create("2008-09-15T15:53:00");

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_string()
        {
            var value = JsonValue.Create("my-string");

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_boolean()
        {
            var value = JsonValue.Create(true);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_number()
        {
            var value = JsonValue.Create(123);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_double_number()
        {
            var value = JsonValue.Create(123.5);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_array()
        {
            var value = JsonValue.Array(1, 2);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_object()
        {
            var value =
                new JsonObject()
                    .Add("1", 1)
                    .Add("2", 1);

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_complex_object()
        {
            var value =
                new JsonObject()
                    .Add("1",
                        JsonValue.Array(
                            new JsonObject().Add("1_1", 11),
                            new JsonObject().Add("1_2", 12)))
                    .Add("2",
                        new JsonObject().Add("2_1", 11));

            var serialized = value.SerializeAndDeserialize();

            Assert.Equal(value, serialized);
        }
    }
}
