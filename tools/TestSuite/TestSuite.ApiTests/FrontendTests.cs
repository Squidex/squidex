// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Codeuctivity.ImageSharpCompare;
using PuppeteerSharp;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public sealed class FrontendTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; }

    public FrontendTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Theory]
    [InlineData("Frontend_Home", "")]
    [InlineData("Frontend_Login", "identity-server/account/login")]
    public async Task Should_render_properly(string name, string url)
    {
        using (var browserFetcher = new BrowserFetcher())
        {
            await browserFetcher.DownloadAsync();
        }

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            DefaultViewport = new ViewPortOptions
            {
                Height = 800,
                IsLandscape = true,
                IsMobile = false,
                Width = 1000
            },
            Args = new string[]
            {
                "--no-sandbox"
            }
        });

        await using var page = await browser.NewPageAsync();

        await page.GoToAsync(_.ClientManager.Options.Url + url + "?skip-setup");
        await page.ScreenshotAsync($"__{name}.jpg");

        var diff = ImageSharpCompare.CalcDiff($"__{name}.jpg", $"Assets/{name}.jpg");

        Assert.InRange(diff.MeanError, 0, 10);
    }
}
