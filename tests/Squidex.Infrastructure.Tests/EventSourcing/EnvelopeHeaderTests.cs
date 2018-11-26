// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EnvelopeHeaderTests
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
            var source = new JsonObject().Add("Key1", 123);
            var headers = new EnvelopeHeaders(source);

            CompareHeaders(headers, source);
        }

        [Fact]
        public void Should_clone_headers()
        {
            var source = new JsonObject().Add("Key1", 123);
            var headers = new EnvelopeHeaders(source);

            var clone = headers.Clone();

            CompareHeaders(headers, clone);
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
