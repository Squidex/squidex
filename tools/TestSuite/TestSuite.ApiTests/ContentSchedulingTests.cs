// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[Trait("Category", "Slow")]
public class ContentSchedulingTests(ContentFixture fixture) : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_publish_content_at_due_time()
    {
        var content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });

        // STEP 1: Schedule the publishing for the next seconds.
        var changeRequest = new ChangeStatus
        {
            Status = "Published",
            DueTime = DateTime.UtcNow.AddSeconds(1),
        };

        await _.Contents.ChangeStatusAsync(content.Id, changeRequest);


        // STEP 2: Wait until the scheduler has published the content.
        var context = QueryContext.Default.Unpublished();

        using var cts = new CancellationTokenSource(60_000);

        while (!cts.IsCancellationRequested)
        {
            var published = await _.Contents.GetAsync(content.Id, context, cts.Token);

            if (published.Status == "Published")
            {
                return;
            }

            await Task.Delay(500, cts.Token);
        }

        throw new InvalidOperationException("The content has not been published.");
    }
}
