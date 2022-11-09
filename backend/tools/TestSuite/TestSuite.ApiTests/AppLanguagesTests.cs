// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public sealed class AppLanguagesTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();

    public ClientFixture _ { get; }

    public AppLanguagesTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_add_language()
    {
        // STEP 0: Add app.
        await CreateAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync("de");
        await AddLanguageAsync("it");

        var languages_1 = await _.Apps.GetLanguagesAsync(appName);

        Assert.Equal(new string[] { "en", "de", "it" }, languages_1.Items.Select(x => x.Iso2Code).ToArray());

        await Verify(languages_1);
    }

    [Fact]
    public async Task Should_add_custom_language()
    {
        // STEP 0: Add app.
        await CreateAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync("abc");
        await AddLanguageAsync("xyz");

        var languages_1 = await _.Apps.GetLanguagesAsync(appName);

        Assert.Equal(new string[] { "en", "abc", "xyz" }, languages_1.Items.Select(x => x.Iso2Code).ToArray());

        await Verify(languages_1);
    }

    [Fact]
    public async Task Should_update_language()
    {
        // STEP 0: Add app.
        await CreateAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync("de");
        await AddLanguageAsync("it");


        // STEP 3: Update German language.
        var updateRequest = new UpdateLanguageDto
        {
            Fallback = new List<string>
            {
                "it"
            },
            IsOptional = true
        };

        var languages_2 = await _.Apps.PutLanguageAsync(appName, "de", updateRequest);
        var language_2_DE = languages_2.Items.Find(x => x.Iso2Code == "de");

        Assert.Equal(updateRequest.Fallback, language_2_DE.Fallback);
        Assert.Equal(updateRequest.IsOptional, language_2_DE.IsOptional);

        await Verify(languages_2);
    }

    [Fact]
    public async Task Should_update_master_language()
    {
        // STEP 0: Add app.
        await CreateAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync("de");
        await AddLanguageAsync("it");


        // STEP 2: Update Italian language.
        var updateRequest = new UpdateLanguageDto
        {
            Fallback = new List<string>
            {
                "de"
            },
            IsOptional = true
        };

        await _.Apps.PutLanguageAsync(appName, "it", updateRequest);


        // STEP 3: Change master language to Italian.
        var masterRequest = new UpdateLanguageDto
        {
            IsMaster = true
        };

        var languages_4 = await _.Apps.PutLanguageAsync(appName, "it", masterRequest);
        var language_4_IT = languages_4.Items.Find(x => x.Iso2Code == "it");
        var language_4_EN = languages_4.Items.Find(x => x.Iso2Code == "en");

        Assert.True(language_4_IT.IsMaster);

        // Old master language is unset.
        Assert.False(language_4_EN.IsMaster);

        // Master language cannot be optional.
        Assert.False(language_4_IT.IsOptional);

        // Fallback for new master language must be removed.
        Assert.Empty(language_4_IT.Fallback);

        await Verify(languages_4);
    }

    [Fact]
    public async Task Should_delete_language()
    {
        // STEP 0: Add app.
        await CreateAppAsync();


        // STEP 1: Add languages.
        await AddLanguageAsync("de");
        await AddLanguageAsync("it");


        // STEP 2: Update Italian language.
        var updateRequest = new UpdateLanguageDto
        {
            Fallback = new List<string>
            {
                "de"
            },
            IsOptional = true
        };

        await _.Apps.PutLanguageAsync(appName, "it", updateRequest);


        // STEP 3: Remove language.
        var languages_2 = await _.Apps.DeleteLanguageAsync(appName, "de");
        var language_2_IT = languages_2.Items.Find(x => x.Iso2Code == "it");

        // Fallback language must be removed.
        Assert.Empty(language_2_IT.Fallback);

        Assert.Equal(new string[] { "en", "it" }, languages_2.Items.Select(x => x.Iso2Code).ToArray());

        await Verify(languages_2);
    }

    private async Task CreateAppAsync()
    {
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        await _.Apps.PostAppAsync(createRequest);
    }

    private async Task AddLanguageAsync(string code)
    {
        var createRequest = new AddLanguageDto
        {
            Language = code
        };

        await _.Apps.PostLanguageAsync(appName, createRequest);
    }
}
