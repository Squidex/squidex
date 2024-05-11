// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Codeuctivity.ImageSharpCompare;
using Microsoft.Playwright;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public sealed class FrontendTests : IClassFixture<ClientFixture>, IClassFixture<PlaywrightFixture>
{
    public ClientFixture _ { get; }

    public IBrowser Browser { get; }

    public FrontendTests(ClientFixture fixture, PlaywrightFixture playwright)
    {
        _ = fixture;

        Browser = playwright.Browser;
    }

    [Theory]
    [InlineData("Frontend_Home", "")]
    [InlineData("Frontend_Login", "identity-server/account/login")]
    public async Task Should_render_properly(string name, string url)
    {
        var page = await Browser.NewPageAsync();

        await page.GotoAsync(_.Client.Options.Url + url + "?skip-setup");
        await page.WaitForLoadStateAsync();

        var screenshot = await page.ScreenshotAsync();

        Directory.CreateDirectory("screenshots");

        var path = $"screenshots/{name}.jpg";

        await File.WriteAllBytesAsync(path, screenshot);

        var diff = ImageSharpCompare.CalcDiff(path, $"Assets/{name}.jpg");

        Assert.InRange(diff.MeanError, 0, 10);
    }
}
