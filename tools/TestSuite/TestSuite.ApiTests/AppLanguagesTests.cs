// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public sealed class AppLanguagesTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; }

    public AppLanguagesTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_add_language()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync(app, "de");
        await AddLanguageAsync(app, "it");

        var languages_1 = await app.Apps.GetLanguagesAsync();

        Assert.Equal(new[] { "en", "de", "it" }, languages_1.Items.Select(x => x.Iso2Code).ToArray());

        await Verify(languages_1);
    }

    [Fact]
    public async Task Should_add_custom_language()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync(app, "abc");
        await AddLanguageAsync(app, "xyz");

        var languages_1 = await app.Apps.GetLanguagesAsync();

        Assert.Equal(new[] { "en", "abc", "xyz" }, languages_1.Items.Select(x => x.Iso2Code).ToArray());

        await Verify(languages_1);
    }

    [Fact]
    public async Task Should_update_language()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync(app, "de");
        await AddLanguageAsync(app, "it");


        // STEP 3: Update German language.
        var updateRequest = new UpdateLanguageDto
        {
            Fallback =
            [
                "it"
            ],
            IsOptional = true
        };

        var languages_2 = await app.Apps.PutLanguageAsync("de", updateRequest);
        var language_2_DE = languages_2.Items.Find(x => x.Iso2Code == "de");

        Assert.Equal(updateRequest.Fallback, language_2_DE?.Fallback);
        Assert.Equal(updateRequest.IsOptional, language_2_DE?.IsOptional);

        await Verify(languages_2);
    }

    [Fact]
    public async Task Should_update_master_language()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync(app, "de");
        await AddLanguageAsync(app, "it");


        // STEP 2: Update Italian language.
        var updateRequest = new UpdateLanguageDto
        {
            Fallback =
            [
                "de"
            ],
            IsOptional = true
        };

        await app.Apps.PutLanguageAsync("it", updateRequest);


        // STEP 3: Change master language to Italian.
        var masterRequest = new UpdateLanguageDto
        {
            IsMaster = true
        };

        var languages_4 = await app.Apps.PutLanguageAsync("it", masterRequest);
        var language_4_IT = languages_4.Items.Find(x => x.Iso2Code == "it");
        var language_4_EN = languages_4.Items.Find(x => x.Iso2Code == "en");

        Assert.True(language_4_IT?.IsMaster);

        // Old master language is unset.
        Assert.False(language_4_EN?.IsMaster);

        // Master language cannot be optional.
        Assert.False(language_4_IT?.IsOptional);

        // Fallback for new master language must be removed.
        Assert.Empty(language_4_IT?.Fallback!);

        await Verify(languages_4);
    }

    [Fact]
    public async Task Should_delete_language()
    {
        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync(app, "de");
        await AddLanguageAsync(app, "it");


        // STEP 2: Update Italian language.
        var updateRequest = new UpdateLanguageDto
        {
            Fallback =
            [
                "de"
            ],
            IsOptional = true
        };

        await app.Apps.PutLanguageAsync("it", updateRequest);


        // STEP 3: Remove language.
        var languages_2 = await app.Apps.DeleteLanguageAsync("de");
        var language_2_IT = languages_2.Items.Find(x => x.Iso2Code == "it");

        // Fallback language must be removed.
        Assert.Empty(language_2_IT?.Fallback!);
        Assert.Equal(new[] { "en", "it" }, languages_2?.Items.Select(x => x.Iso2Code).ToArray());

        await Verify(languages_2);
    }

    private static async Task AddLanguageAsync(ISquidexClient app, string code)
    {
        var createRequest = new AddLanguageDto
        {
            Language = code
        };

        await app.Apps.PostLanguageAsync(createRequest);
    }
}
