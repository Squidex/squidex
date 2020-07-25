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
            var (result, notFound) = sut.Get(CultureInfo.CurrentUICulture, "key");

            Assert.Equal("key", result);
            Assert.True(notFound);
        }

        [Fact]
        public void Should_return_simple_key()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "simple");

            Assert.Equal("Simple Result", result);
        }

        [Fact]
        public void Should_return_text_with_variable()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "withVar", new { var = 5 });

            Assert.Equal("Var: 5.", result);
        }

        [Fact]
        public void Should_return_text_with_translated_variable()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "withTranslatedVar", new { var = "simple" });

            Assert.Equal("Var: Simple Result.", result);
        }

        [Fact]
        public void Should_return_text_with_lower_var()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "withLowerVar", new { var = "Lower" });

            Assert.Equal("Var: lower.", result);
        }

        [Fact]
        public void Should_return_text_with_upper_var()
        {
            var (result, _) = sut.Get(CultureInfo.CurrentUICulture, "withUpperVar", new { var = "upper" });

            Assert.Equal("Var: Upper.", result);
        }
    }
}
