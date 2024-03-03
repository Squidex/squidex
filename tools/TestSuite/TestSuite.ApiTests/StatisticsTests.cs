// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class StatisticsTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; }

    public StatisticsTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_get_logs()
    {
        // STEP 1: Get initial log response.
        var log = await _.Client.Statistics.GetLogAsync();


        // STEP 2: Download log.
        var httpClient = _.Client.CreateHttpClient();
        var httpResponse = await httpClient.GetAsync(log.DownloadUrl);

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.Equal("text/csv", httpResponse.Content.Headers.GetValues("Content-Type").First());
    }

    [Fact]
    public async Task Should_get_api_calls()
    {
        // STEP 1: Get statistics.
        var dateFrom = DateTimeOffset.UtcNow.AddDays(-30);
        var dateTo = DateTimeOffset.UtcNow;

        var result = await _.Client.Statistics.GetUsagesAsync(dateFrom, dateTo);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_get_storage_size()
    {
        // STEP 1: Get statistics.
        var dateFrom = DateTimeOffset.UtcNow.AddDays(-30);
        var dateTo = DateTimeOffset.UtcNow;

        var result = await _.Client.Statistics.GetStorageSizesAsync(dateFrom, dateTo);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_get_current_storage_size()
    {
        // STEP 1: Get statistics.
        var result = await _.Client.Statistics.GetCurrentStorageSizeAsync();

        Assert.NotNull(result);
    }
}
