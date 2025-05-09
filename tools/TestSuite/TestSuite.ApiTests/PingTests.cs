﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class PingTests(CreatedAppFixture fixture) : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_ping_service()
    {
        await _.Client.Ping.GetPingAsync();
    }

    [Fact]
    public async Task Should_ping_app()
    {
        await _.Client.Ping.GetAppPingAsync();
    }

    [Fact]
    public async Task Should_get_info()
    {
        var infos = await _.Client.Ping.GetInfoAsync();

        Assert.NotNull(infos);
    }
}
