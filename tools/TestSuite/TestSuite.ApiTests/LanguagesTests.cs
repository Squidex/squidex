// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class LanguagesTests(ClientFixture fixture) : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_provide_languages()
    {
        var languages = await _.Client.Languages.GetLanguagesAsync();

        Assert.True(languages.Count > 100);
    }
}
