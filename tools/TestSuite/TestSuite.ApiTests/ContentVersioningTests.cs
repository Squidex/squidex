// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class ContentVersioningTests(ContentFixture fixture) : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_get_old_version()
    {
        var content_0 = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, ContentCreateOptions.AsPublish);

        // STEP 1: Update the content.
        var content_1 = await _.Contents.UpdateAsync(content_0.Id, new TestEntityData { Number = 2 });

        Assert.Equal(2, content_1.Data.Number);


        // STEP 2: Get the old version of the content.
        var content_2 = await _.Contents.GetAsync(content_0.Id, content_0.Version);

        Assert.Equal(1, content_2.Data.Number);


        // STEP 3: Get an unknown version and fail.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.GetAsync(content_0.Id, 1000);
        });

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Should_delete_draft()
    {
        var content_0 = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, ContentCreateOptions.AsPublish);

        // STEP 1: Create a draft version.
        await _.Contents.CreateDraftAsync(content_0.Id);

        var content_1 = await _.Contents.UpdateAsync(content_0.Id, new TestEntityData { Number = 2 });

        Assert.Equal(2, content_1.Data.Number);


        // STEP 2: Delete the draft version.
        var content_2 = await _.Contents.DeleteDraftAsync(content_0.Id);

        // Should return the published value again.
        Assert.Equal(1, content_2.Data.Number);
    }

    [Fact]
    public async Task Should_cancel_scheduled_status()
    {
        var content_0 = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });

        // STEP 1: Schedule the publishing.
        var changeRequest = new ChangeStatus
        {
            Status = "Published",
            DueTime = DateTime.UtcNow.AddDays(1),
        };

        await _.Contents.ChangeStatusAsync(content_0.Id, changeRequest);

        using var httpClient = _.Client.CreateHttpClient();

        // The content is not published yet, therefore the header is needed.
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Unpublished", "1");

        var url = $"{_.Client.Options.Url}api/content/{_.AppName}/{_.SchemaName}/{content_0.Id}";

        // The schedule job is not part of the typed model, therefore we query the raw content.
        var content_1 = await GetJsonAsync(httpClient, url);

        Assert.NotNull(content_1["scheduleJob"]?.Value<string>("status"));


        // STEP 2: Cancel the scheduled status change.
        var response = await httpClient.DeleteAsync($"{url}/status");

        response.EnsureSuccessStatusCode();

        var content_2 = await GetJsonAsync(httpClient, url);

        Assert.Null(content_2["scheduleJob"]?.Value<string>("status"));
    }

    private static async Task<JObject> GetJsonAsync(HttpClient httpClient, string url)
    {
        var response = await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return JObject.Parse(await response.Content.ReadAsStringAsync());
    }
}
