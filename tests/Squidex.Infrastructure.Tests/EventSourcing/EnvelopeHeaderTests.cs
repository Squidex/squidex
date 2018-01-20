// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    public class EnvelopeHeaderTests
    {
        [Fact]
        public void Should_create_headers()
        {
            var headers = new EnvelopeHeaders();

            Assert.Equal(0, headers.Count);
        }

        [Fact]
        public void Should_create_headers_with_null_properties()
        {
            var headers = new EnvelopeHeaders(null);

            Assert.Equal(0, headers.Count);
        }

        [Fact]
        public void Should_create_headers_as_copy()
        {
            var source = new PropertiesBag().Set("Key1", 123);
            var headers = new EnvelopeHeaders(source);

            CompareHeaders(headers, source);
        }

        [Fact]
        public void Should_clone_headers()
        {
            var source = new PropertiesBag().Set("Key1", 123);
            var headers = new EnvelopeHeaders(source);

            var clone = headers.Clone();

            CompareHeaders(headers, clone);
        }

        private static void CompareHeaders(PropertiesBag lhs, PropertiesBag rhs)
        {
            foreach (var key in lhs.PropertyNames.Concat(rhs.PropertyNames).Distinct())
            {
                Assert.Equal(lhs[key].ToString(), rhs[key].ToString());
            }
        }
    }
}
