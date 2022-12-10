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

public class ContentReferencesTests : IClassFixture<ContentReferencesFixture>
{
    public ContentReferencesFixture _ { get; }

    public ContentReferencesTests(ContentReferencesFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_not_deliver_unpublished_references()
    {
        // STEP 1: Create a referenced content.
        var contentA_1 = await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = null
        });


        // STEP 2: Create a content with a reference.
        var contentB_1 = await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = new[] { contentA_1.Id }
        }, ContentCreateOptions.AsPublish);


        // STEP 3: Query new item
        var contentB_2 = await _.Contents.GetAsync(contentB_1.Id);

        Assert.Empty(contentB_2.Data.References);


        // STEP 4: Publish reference
        await _.Contents.ChangeStatusAsync(contentA_1.Id, new ChangeStatus
        {
            Status = "Published"
        });


        // STEP 5: Query new item again
        var contentB_3 = await _.Contents.GetAsync(contentB_1.Id);

        Assert.Equal(new string[] { contentA_1.Id }, contentB_3.Data.References);
    }

    [Fact]
    public async Task Should_not_delete_when_referenced()
    {
        // STEP 1: Create a referenced content.
        var contentA_1 = await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = null
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Create a content with a reference.
        await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = new[] { contentA_1.Id }
        }, ContentCreateOptions.AsPublish);


        // STEP 3: Try to delete with referrer check.
        var options = new ContentDeleteOptions { CheckReferrers = true };

        await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.DeleteAsync(contentA_1.Id, options);
        });


        // STEP 4: Delete without referrer check
        await _.Contents.DeleteAsync(contentA_1.Id);
    }

    [Fact]
    public async Task Should_not_unpublish_when_referenced()
    {
        // STEP 1: Create a published referenced content.
        var contentA_1 = await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = null
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Create a content with a reference.
        await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = new[] { contentA_1.Id }
        }, ContentCreateOptions.AsPublish);


        // STEP 3: Try to ThrowsAnyAsync with referrer check.
        await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.ChangeStatusAsync(contentA_1.Id, new ChangeStatus
            {
                Status = "Draft",
                // Ensure that the flag is true.
                CheckReferrers = true
            });
        });


        // STEP 4: Delete without referrer check
        await _.Contents.ChangeStatusAsync(contentA_1.Id, new ChangeStatus
        {
            Status = "Draft",
            // It is the default anyway, just to make it more explicit.
            CheckReferrers = false
        });
    }

    [Fact]
    public async Task Should_not_delete_with_bulk_when_referenced()
    {
        // STEP 1: Create a referenced content.
        var contentA_1 = await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = null
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Create a content with a reference.
        await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = new[] { contentA_1.Id }
        }, ContentCreateOptions.AsPublish);


        // STEP 3: Try to delete with referrer check.
        var result1 = await _.Contents.BulkUpdateAsync(new BulkUpdate
        {
            Jobs = new List<BulkUpdateJob>
            {
                new BulkUpdateJob
                {
                    Id = contentA_1.Id,
                    Type = BulkUpdateType.Delete,
                    Status = "Draft"
                }
            },
            CheckReferrers = true
        });

        Assert.NotNull(result1[0].Error);


        // STEP 4: Delete without referrer check
        var result2 = await _.Contents.BulkUpdateAsync(new BulkUpdate
        {
            Jobs = new List<BulkUpdateJob>
            {
                new BulkUpdateJob
                {
                    Id = contentA_1.Id,
                    Type = BulkUpdateType.Delete,
                    Status = "Draft"
                }
            },
            CheckReferrers = false
        });

        Assert.Null(result2[0].Error);
    }

    [Fact]
    public async Task Should_not_unpublish_with_bulk_when_referenced()
    {
        // STEP 1: Create a published referenced content.
        var contentA_1 = await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = null
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Create a published content with a reference.
        await _.Contents.CreateAsync(new TestEntityWithReferencesData
        {
            References = new[] { contentA_1.Id }
        }, ContentCreateOptions.AsPublish);


        // STEP 3: Try to delete with referrer check.
        var result1 = await _.Contents.BulkUpdateAsync(new BulkUpdate
        {
            Jobs = new List<BulkUpdateJob>
            {
                new BulkUpdateJob
                {
                    Id = contentA_1.Id,
                    Type = BulkUpdateType.ChangeStatus,
                    Status = "Draft"
                }
            },
            CheckReferrers = true
        });

        Assert.NotNull(result1[0].Error);


        // STEP 4: Delete without referrer check
        var result2 = await _.Contents.BulkUpdateAsync(new BulkUpdate
        {
            Jobs = new List<BulkUpdateJob>
            {
                new BulkUpdateJob
                {
                    Id = contentA_1.Id,
                    Type = BulkUpdateType.ChangeStatus,
                    Status = "Draft"
                }
            },
            CheckReferrers = false
        });

        Assert.Null(result2[0].Error);
    }
}
