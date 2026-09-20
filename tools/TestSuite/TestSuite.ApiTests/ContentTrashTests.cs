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

public class ContentTrashTests(ContentReferencesFixture fixture) : IClassFixture<ContentReferencesFixture>
{
    public ContentReferencesFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_not_return_deleted_content()
    {
        var content = await CreateAsync();

        // STEP 1: Delete the content.
        await _.Contents.DeleteAsync(content.Id);


        // STEP 2: Query the content by ID and fail.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.GetAsync(content.Id);
        });

        Assert.Equal(404, ex.StatusCode);


        // STEP 3: The deleted content is not part of the results anymore.
        var contents = await _.Contents.GetAsync(new ContentQuery { Ids = [content.Id] });

        Assert.Empty(contents.Items);


        // STEP 4: The deleted content is not streamed anymore.
        var streamed = new List<string>();

        await _.Contents.StreamAllAsync(item =>
        {
            streamed.Add(item.Id);
            return Task.CompletedTask;
        });

        Assert.DoesNotContain(content.Id, streamed);
    }

    [Fact]
    public async Task Should_delete_content_when_referrer_is_deleted()
    {
        // STEP 1: Create a referenced content and a content with a reference.
        var contentA = await CreateAsync();
        var contentB = await CreateAsync(contentA.Id);

        var referencing_1 = await _.Contents.GetReferencingAsync(contentA.Id);

        Assert.Contains(referencing_1.Items, x => x.Id == contentB.Id);


        // STEP 2: Delete the referrer.
        await _.Contents.DeleteAsync(contentB.Id);

        var referencing_2 = await _.Contents.GetReferencingAsync(contentA.Id);

        Assert.DoesNotContain(referencing_2.Items, x => x.Id == contentB.Id);


        // STEP 3: Delete the referenced content with the referrer check.
        var options = new ContentDeleteOptions { CheckReferrers = true };

        // A deleted content must not block the deletion of the referenced content.
        await _.Contents.DeleteAsync(contentA.Id, options);
    }

    private async Task<TestEntityWithReferences> CreateAsync(params string[] references)
    {
        return await _.Contents.CreateAsync(
            new TestEntityWithReferencesData
            {
                References = references.Length > 0 ? references : null,
            },
            ContentCreateOptions.AsPublish);
    }
}
