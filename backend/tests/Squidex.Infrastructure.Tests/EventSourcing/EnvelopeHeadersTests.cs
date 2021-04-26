// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EnvelopeHeadersTests
    {
        [Fact]
        public void Should_create_headers()
        {
            var headers = new EnvelopeHeaders();

            Assert.Empty(headers);
        }

        [Fact]
        public void Should_create_headers_as_copy()
        {
            var source = JsonValue.Object().Add("Key1", 123);

            var headers = new EnvelopeHeaders(source);

            CompareHeaders(headers, source);
        }

        [Fact]
        public void Should_clone_headers()
        {
            var headers = new EnvelopeHeaders(JsonValue.Object().Add("Key1", 123));

            var clone = headers.CloneHeaders();

            CompareHeaders(headers, clone);
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var value = new EnvelopeHeaders(JsonValue.Object().Add("Key1", 123));

            var deserialized = value.SerializeAndDeserialize();

            CompareHeaders(deserialized, value);
        }

        private static void CompareHeaders(JsonObject lhs, JsonObject rhs)
        {
            foreach (var key in lhs.Keys.Concat(rhs.Keys).Distinct())
            {
                Assert.Equal(lhs[key].ToString(), rhs[key].ToString());
            }
        }
    }
}
