// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class TeamStatisticsTests : IClassFixture<CreatedTeamFixture>
{
    public CreatedTeamFixture _ { get; }

    public TeamStatisticsTests(CreatedTeamFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_get_api_calls()
    {
        // STEP 1: Get statistics.
        var dateFrom = DateTimeOffset.UtcNow.AddDays(-30);
        var dateTo = DateTimeOffset.UtcNow;

        var result = await _.Client.Statistics.GetUsagesForTeamAsync(_.TeamId, dateFrom, dateTo);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_get_storage_size()
    {
        // STEP 1: Get statistics.
        var dateFrom = DateTimeOffset.UtcNow.AddDays(-30);
        var dateTo = DateTimeOffset.UtcNow;

        var result = await _.Client.Statistics.GetStorageSizesForTeamAsync(_.TeamId, dateFrom, dateTo);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_get_current_storage_size_for()
    {
        // STEP 1: Get statistics.
        var result = await _.Client.Statistics.GetTeamCurrentStorageSizeForTeamAsync(_.TeamId);

        Assert.NotNull(result);
    }
}
