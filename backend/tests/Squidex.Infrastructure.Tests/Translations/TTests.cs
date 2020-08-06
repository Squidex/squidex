// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Xunit;

namespace Squidex.Infrastructure.Translations
{
    public class TTests
    {
        private readonly ILocalizer sut;

        public TTests()
        {
            sut = new ResourcesLocalizer(SampleResources.ResourceManager);
        }

        [Fact]
        public void Should_return_key_if_not_found()
        {
            var (result, notFound) = sut.Get(CultureInfo.CurrentUICulture, "key", "fallback");

            Assert.Equal("fallback", result);
            Assert.True(notFound);
        }

        [Fact]
        public void Should_return_simple_key()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "simple", "fallback");

            Assert.Equal("Simple Result", result);
        }

        [Fact]
        public void Should_return_text_with_variable()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "withVar", "fallback", new { var = 5 });

            Assert.Equal("Var: 5.", result);
        }

        [Fact]
        public void Should_return_text_with_lower_variable()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "withLowerVar", "fallback", new { var = "Lower" });

            Assert.Equal("Var: lower.", result);
        }

        [Fact]
        public void Should_return_text_with_upper_variable()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "withUpperVar", "fallback", new { var = "upper" });

            Assert.Equal("Var: Upper.", result);
        }
    }
}
