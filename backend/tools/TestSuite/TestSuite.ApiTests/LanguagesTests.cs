// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests
{
    public class LanguagesTests : IClassFixture<ClientFixture>
    {
        public ClientFixture _ { get; }

        public LanguagesTests(ClientFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_provide_languages()
        {
            var languages = await _.Languages.GetLanguagesAsync();

            Assert.True(languages.Count > 100);
        }
    }
}
