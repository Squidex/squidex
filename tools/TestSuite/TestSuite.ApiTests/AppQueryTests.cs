// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class AppQueryTests(ClientFixture fixture) : IClassFixture<ClientFixture>
{
    private readonly string appPrefix = Guid.NewGuid().ToString();

    public ClientFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_query_all_apps_by_name()
    {
        // STEP 1: Create app.
        var appName = $"{appPrefix}-1";

        await _.PostAppAsync(appName);


        // STEP 2: Query apps by name.
        var apps = await _.Client.Apps.GetAppsV2Async(appName);

        // Should return all apps, because the client is an administrator.
        Assert.True(apps.ContainsAllApps);
        Assert.Equal(1, apps.Total);
        Assert.Equal(appName, apps.Items.Single().Name);
    }

    [Fact]
    public async Task Should_query_all_apps_with_paging()
    {
        // STEP 1: Create apps.
        var appName1 = $"{appPrefix}-1";
        var appName2 = $"{appPrefix}-2";

        await _.PostAppAsync(appName1);
        await _.PostAppAsync(appName2);


        // STEP 2: Query first page.
        var page1 = await _.Client.Apps.GetAppsV2Async(appPrefix, skip: 0, take: 1);

        Assert.Equal(2, page1.Total);
        Assert.Equal(appName1, page1.Items.Single().Name);


        // STEP 3: Query second page.
        var page2 = await _.Client.Apps.GetAppsV2Async(appPrefix, skip: 1, take: 1);

        Assert.Equal(2, page2.Total);
        Assert.Equal(appName2, page2.Items.Single().Name);
    }

    [Fact]
    public async Task Should_not_query_archived_apps()
    {
        // STEP 1: Create app.
        var appName = $"{appPrefix}-1";

        var (app, _) = await _.PostAppAsync(appName);


        // STEP 2: Archive app.
        await app.Apps.DeleteAppAsync();

        var apps = await _.Client.Apps.GetAppsV2Async(appName);

        // Should not provide deleted app when apps are queried.
        Assert.Equal(0, apps.Total);
        Assert.Empty(apps.Items);
    }
}
