// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public sealed class AppLanguagesTests : IClassFixture<ClientFixture>
    {
        public ClientFixture _ { get; }

        public AppLanguagesTests(ClientFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_manage_languages()
        {
            var appName = Guid.NewGuid().ToString();

            // STEP 1: Add app
            var createRequest = new CreateAppDto { Name = appName };

            await _.Apps.PostAppAsync(createRequest);


            // STEP 2: Add languages
            await _.Apps.PostLanguageAsync(appName, new AddLanguageDto { Language = "de" });
            await _.Apps.PostLanguageAsync(appName, new AddLanguageDto { Language = "it" });
            await _.Apps.PostLanguageAsync(appName, new AddLanguageDto { Language = "fr" });

            var languages = await _.Apps.GetLanguagesAsync(appName);

            var languageEN = languages.Items.First(x => x.Iso2Code == "en");

            Assert.Equal(new string[] { "en", "de", "fr", "it" }, languages.Items.Select(x => x.Iso2Code).ToArray());
            Assert.True(languageEN.IsMaster);


            // STEP 3: Update language1
            var updateRequest1 = new UpdateLanguageDto
            {
                Fallback = new string[]
                {
                    "fr",
                    "it"
                },
                IsOptional = true
            };

            languages = await _.Apps.PutLanguageAsync(appName, "de", updateRequest1);

            var languageDE = languages.Items.First(x => x.Iso2Code == "de");

            Assert.Equal(new string[] { "fr", "it" }, languageDE.Fallback.ToArray());
            Assert.True(languageDE.IsOptional);


            // STEP 4: Update language2
            var updateRequest2 = new UpdateLanguageDto
            {
                Fallback = new string[]
                {
                    "fr",
                    "it"
                }
            };

            languages = await _.Apps.PutLanguageAsync(appName, "en", updateRequest2);

            languageEN = languages.Items.First(x => x.Iso2Code == "en");

            Assert.Equal(new string[] { "fr", "it" }, updateRequest2.Fallback.ToArray());


            // STEP 5: Change master language
            var masterRequest = new UpdateLanguageDto { IsMaster = true };

            languages = await _.Apps.PutLanguageAsync(appName, "de", masterRequest);

            languageDE = languages.Items.First(x => x.Iso2Code == "de");
            languageEN = languages.Items.First(x => x.Iso2Code == "en");

            Assert.True(languageDE.IsMaster);
            Assert.False(languageDE.IsOptional);
            Assert.False(languageEN.IsMaster);
            Assert.Empty(languageDE.Fallback);
            Assert.Equal(new string[] { "de", "en", "fr", "it" }, languages.Items.Select(x => x.Iso2Code).ToArray());


            // STEP 6: RemoveRequest
            languages = await _.Apps.DeleteLanguageAsync(appName, "fr");

            languageEN = languages.Items.First(x => x.Iso2Code == "en");

            Assert.Equal(new string[] { "it" }, languageEN.Fallback.ToArray());
            Assert.Equal(new string[] { "de", "en", "it" }, languages.Items.Select(x => x.Iso2Code).ToArray());
        }
    }
}
