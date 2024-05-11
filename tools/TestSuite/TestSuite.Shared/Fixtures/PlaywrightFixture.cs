// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Playwright;
using Xunit;

namespace TestSuite.Fixtures;

public class PlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; set; } = null!;

    private IPlaywright Instance { get; set; } = null!;

    public async Task InitializeAsync()
    {
        Instance = await Playwright.CreateAsync();

        Browser = await Instance.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();

        Instance.Dispose();
    }
}
