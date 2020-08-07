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
            var result = sut.Get(CultureInfo.CurrentUICulture, "key", "fallback");

            Assert.Equal(("fallback", false), result);
        }

        [Fact]
        public void Should_return_simple_key()
        {
            var result = sut.Get(CultureInfo.CurrentUICulture, "simple", "fallback");

            Assert.Equal(("Simple Result", true), result);
        }

        [Fact]
        public void Should_return_text_with_variable()
        {
            var result = sut.Get(CultureInfo.CurrentUICulture, "withVar", "fallback", new { var = 5 });

            Assert.Equal(("Var: 5.", true), result);
        }

        [Fact]
        public void Should_return_text_with_lower_variable()
        {
            var result = sut.Get(CultureInfo.CurrentUICulture, "withLowerVar", "fallback", new { var = "Lower" });

            Assert.Equal(("Var: lower.", true), result);
        }

        [Fact]
        public void Should_return_text_with_upper_variable()
        {
            var result = sut.Get(CultureInfo.CurrentUICulture, "withUpperVar", "fallback", new { var = "upper" });

            Assert.Equal(("Var: Upper.", true), result);
        }
    }
}
