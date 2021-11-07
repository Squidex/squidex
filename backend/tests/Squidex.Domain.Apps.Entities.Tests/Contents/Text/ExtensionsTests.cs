// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class ExtensionsTests
    {
        [Fact]
        public void Should_prefix_field_query()
        {
            var source = "en:Hello";

            Assert.Equal("texts.en:Hello", source.PrefixField("texts."));
        }

        [Fact]
        public void Should_prefix_field_query_with_nothing()
        {
            var source = "en:Hello";

            Assert.Equal("en:Hello", source.PrefixField(string.Empty));
        }

        [Fact]
        public void Should_prefix_field_with_complex_language()
        {
            var source = "en-EN:Hello";

            Assert.Equal("texts.en-EN:Hello", source.PrefixField("texts."));
        }

        [Fact]
        public void Should_prefix_field_with_complex_language_and_underscoring()
        {
            var source = "en-EN:Hello";

            Assert.Equal("texts.en_EN:Hello", source.PrefixField("texts.", true));
        }

        [Fact]
        public void Should_prefix_field_query_within_query()
        {
            var source = "Hello en:World";

            Assert.Equal("Hello texts.en:World", source.PrefixField("texts."));
        }

        [Fact]
        public void Should_prefix_field_query_within_complex_query()
        {
            var source = "Hallo OR (Hello en:World)";

            Assert.Equal("Hallo OR (Hello texts.en:World)", source.PrefixField("texts."));
        }
    }
}
