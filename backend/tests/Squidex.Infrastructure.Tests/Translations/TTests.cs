// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.Translations
{
    public class TTests
    {
        public TTests()
        {
            T.Setup(SampleResources.ResourceManager);
        }

        [Fact]
        public void Should_return_key_if_not_initialized()
        {
            T.Setup(null!);

            var result = T.Get("key");

            Assert.Equal("key", result);
        }

        [Fact]
        public void Should_return_key_if_not_found()
        {
            var result = T.Get("key");

            Assert.Equal("key", result);
        }

        [Fact]
        public void Should_return_simple_key()
        {
            var result = T.Get("simple");

            Assert.Equal("Simple Result", result);
        }

        [Fact]
        public void Should_return_text_with_variable()
        {
            var result = T.Get("withVar", new { var = 5 });

            Assert.Equal("Var: 5.", result);
        }

        [Fact]
        public void Should_return_text_with_translated_variable()
        {
            var result = T.Get("withTranslatedVar", new { var = "simple" });

            Assert.Equal("Var: Simple Result.", result);
        }

        [Fact]
        public void Should_return_text_with_lower_var()
        {
            var result = T.Get("withLowerVar", new { var = "Lower" });

            Assert.Equal("Var: lower.", result);
        }

        [Fact]
        public void Should_return_text_with_upper_var()
        {
            var result = T.Get("withUpperVar", new { var = "upper" });

            Assert.Equal("Var: Upper.", result);
        }
    }
}
