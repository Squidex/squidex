// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.EnrichedEvents;
using TestSuite.Model;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public class ContentUpdateTests : IClassFixture<ContentFixture>
{
    public ContentFixture _ { get; }

    public ContentUpdateTests(ContentFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_return_published_content()
    {
        // STEP 1: Create the item unpublished.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        });


        // STEP 2: Publish the item.
        await _.Contents.ChangeStatusAsync(content.Id, new ChangeStatus
        {
            Status = "Published"
        });


        // STEP 3: Retrieve the item.
        await _.Contents.GetAsync(content.Id);
    }

    [Fact]
    public async Task Should_not_return_archived_content()
    {
        // STEP 1: Create the item published.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Archive the item.
        await _.Contents.ChangeStatusAsync(content.Id, new ChangeStatus
        {
            Status = "Archived"
        });


        // STEP 3. Get a 404 for the item because it is not published anymore.
        await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.GetAsync(content.Id);
        });
    }

    [Fact]
    public async Task Should_not_return_unpublished_content()
    {
        // STEP 1: Create the item unpublished.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        });


        // STEP 2: Change the status to publiushed and then to draft.
        await _.Contents.ChangeStatusAsync(content.Id, new ChangeStatus
        {
            Status = "Published"
        });
        await _.Contents.ChangeStatusAsync(content.Id, new ChangeStatus
        {
            Status = "Draft"
        });


        // STEP 3. Get a 404 for the item because it is not published anymore.
        await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.GetAsync(content.Id);
        });
    }

    [Fact]
    public async Task Should_create_strange_text()
    {
        const string text = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

        // STEP 1: Create a content item with a text that caused a bug before.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = text
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Get the item and ensure that the text is the same.
        var queried = await _.Contents.GetAsync(content.Id);

        Assert.Equal(text, queried.Data.String);

        await Verify(queried);
    }

    [Fact]
    public async Task Should_create_null_localized_text()
    {
        // STEP 1: Create a content item with a text that caused a bug before.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Localized = new Dictionary<string, string>
            {
                ["en"] = null
            }
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Get the item and ensure that the text is the same.
        var queried = await _.Contents.GetAsync(content.Id, QueryContext.Default.IgnoreFallback());

        Assert.Null(queried.Data.Localized["en"]);

        await Verify(queried);
    }

    [Fact]
    public async Task Should_create_json_with_dot()
    {
        // STEP 1: Create a content item with a text that caused a bug before.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Json = new JObject
            {
                ["field.with.dot"] = 42
            }
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Get the item and ensure that the text is the same.
        var queried = await _.Contents.GetAsync(content.Id, QueryContext.Default.IgnoreFallback());

        Assert.Equal(42, (int)queried.Data.Json["field.with.dot"]);

        await Verify(queried);
    }

    [Fact]
    public async Task Should_create_default_text()
    {
        // STEP 1: Create a content item with a text that caused a bug before.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Localized = new Dictionary<string, string>()
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Get the item and ensure that the text is the same.
        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal("default", updated.Data.Localized["en"]);
    }

    [Fact]
    public async Task Should_create_non_published_content()
    {
        // STEP 1: Create the item unpublished.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        });


        // STEP 2. Get a 404 for the item because it is not published.
        await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.GetAsync(content.Id);
        });

        await Verify(content);
    }

    [Fact]
    public async Task Should_create_published_content()
    {
        // STEP 1: Create the item published.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Get the item.
        await _.Contents.GetAsync(content.Id);

        await Verify(content);
    }

    [Fact]
    public async Task Should_create_draft_version()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Create draft.
        content = await _.Contents.CreateDraftAsync(content.Id);


        // STEP 3: Update the item and ensure that the data has not changed.
        await _.Contents.PatchAsync(content.Id, new TestEntityData
        {
            Number = 2
        });

        var updated_1 = await _.Contents.GetAsync(content.Id);

        Assert.Equal(1, updated_1.Data.Number);


        // STEP 4: Get the unpublished version.
        var unpublished = await _.Contents.GetAsync(content.Id, QueryContext.Default.Unpublished());

        Assert.Equal(2, unpublished.Data.Number);


        // STEP 5: Publish draft and ensure that it has been updated.
        await _.Contents.ChangeStatusAsync(content.Id, new ChangeStatus
        {
            Status = "Published"
        });

        var updated_2 = await _.Contents.GetAsync(content.Id);

        Assert.Equal(2, updated_2.Data.Number);
    }

    [Fact]
    public async Task Should_create_content_with_custom_id()
    {
        var id = $"custom-{Guid.NewGuid()}";

        // STEP 1: Create a new item with a custom id.
        var options = new ContentCreateOptions { Id = id, Publish = true };

        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, options);

        Assert.Equal(id, content.Id);
    }

    [Fact]
    public async Task Should_not_create_content_with_custom_id_twice()
    {
        var id = $"custom-{Guid.NewGuid()}";

        // STEP 1: Create a new item with a custom id.
        var options = new ContentCreateOptions { Id = id, Publish = true };

        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, options);

        Assert.Equal(id, content.Id);


        // STEP 2: Create a new item with a custom id.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Contents.CreateAsync(new TestEntityData
            {
                Number = 1
            }, options);
        });

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Should_create_content_with_custom_id_and_upsert()
    {
        var id = $"custom-{Guid.NewGuid()}";

        // STEP 1: Upsert a new item with a custom id.
        var content = await _.Contents.UpsertAsync(id, new TestEntityData
        {
            Number = 1
        }, ContentUpsertOptions.AsPublish);

        Assert.Equal(id, content.Id);


        // STEP 2: Make an update with the upsert endpoint.
        content = await _.Contents.UpsertAsync(id, new TestEntityData
        {
            Number = 2
        });

        Assert.Equal(2, content.Data.Number);


        // STEP 3: Make an update with the update endpoint.
        content = await _.Contents.UpdateAsync(id, new TestEntityData
        {
            Number = 3
        });

        Assert.Equal(3, content.Data.Number);
    }

    [Theory]
    [InlineData(Strategies.Update.Normal)]
    [InlineData(Strategies.Update.Upsert)]
    [InlineData(Strategies.Update.UpsertBulk)]
    [InlineData(Strategies.Update.Bulk)]
    [InlineData(Strategies.Update.BulkShared)]
    [InlineData(Strategies.Update.BulkWithSchema)]
    public async Task Should_update_content(Strategies.Update strategy)
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = "2"
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Update with selected strategy.
        await _.Client.UpdateAsync(content, new TestEntityData
        {
            Number = 200
        }, strategy);

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal(200, updated.Data.Number);

        // Other data fields are overwritten.
        Assert.Null(updated.Data.String);
    }

    [Theory]
    [InlineData(Strategies.Update.Normal)]
    [InlineData(Strategies.Update.Upsert)]
    [InlineData(Strategies.Update.UpsertBulk)]
    [InlineData(Strategies.Update.Bulk)]
    [InlineData(Strategies.Update.BulkShared)]
    [InlineData(Strategies.Update.BulkWithSchema)]
    public async Task Should_update_content_to_null(Strategies.Update strategy)
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = "initial"
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Update with selected strategy.
        await _.Client.UpdateAsync(content, new TestEntityData
        {
            String = null
        }, strategy);

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Null(updated.Data.String);
    }

    [Theory]
    [InlineData(Strategies.Patch.Normal)]
    [InlineData(Strategies.Patch.Upsert)]
    [InlineData(Strategies.Patch.UpsertBulk)]
    [InlineData(Strategies.Patch.Bulk)]
    [InlineData(Strategies.Patch.BulkShared)]
    [InlineData(Strategies.Patch.BulkWithSchema)]
    public async Task Should_patch_content(Strategies.Patch strategy)
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = "initial"
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Patch with selected strategy.
        await _.Client.PatchAsync(content, new TestEntityData
        {
            Number = 200
        }, strategy);

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal(200, updated.Data.Number);

        // Other data fields cannot be changed.
        Assert.Equal("initial", updated.Data.String);
    }

    [Theory]
    [InlineData(Strategies.Patch.Normal)]
    [InlineData(Strategies.Patch.Upsert)]
    [InlineData(Strategies.Patch.UpsertBulk)]
    [InlineData(Strategies.Patch.Bulk)]
    [InlineData(Strategies.Patch.BulkShared)]
    [InlineData(Strategies.Patch.BulkWithSchema)]
    public async Task Should_patch_id_data_value(Strategies.Patch strategy)
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Id = "id1"
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Patch with selected strategy.
        await _.Client.PatchAsync(content, new TestEntityData
        {
            Id = "id2"
        }, strategy);

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal("id2", updated.Data.Id);
    }

    [Theory]
    [InlineData(Strategies.Patch.Normal)]
    [InlineData(Strategies.Patch.Upsert)]
    [InlineData(Strategies.Patch.UpsertBulk)]
    [InlineData(Strategies.Patch.Bulk)]
    [InlineData(Strategies.Patch.BulkShared)]
    [InlineData(Strategies.Patch.BulkWithSchema)]
    public async Task Should_patch_content_to_null(Strategies.Patch strategy)
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = "initial"
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Patch with selected strategy.
        await _.Client.PatchAsync(content, new
        {
            @string = new { iv = (string)null }
        }, strategy);

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Null(updated.Data.String);
    }

    [Theory]
    [InlineData(Strategies.Deletion.SingleSoft)]
    [InlineData(Strategies.Deletion.SinglePermanent)]
    [InlineData(Strategies.Deletion.BulkSoft)]
    [InlineData(Strategies.Deletion.BulkPermanent)]
    public async Task Should_delete_content(Strategies.Deletion strategy)
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Delete with selected strategy.
        await _.Client.DeleteAsync(content, strategy);


        // STEP 3: Retrieve all items and ensure that the deleted item does not exist.
        var updated = await _.Contents.GetAsync();

        Assert.DoesNotContain(updated.Items, x => x.Id == content.Id);


        // STEP 4: Retrieve all deleted items and check if found.
        var q = new ContentQuery { Filter = "isDeleted eq true" };

        var deleted = await _.Contents.GetAsync(q, QueryContext.Default.Unpublished(true));

        Assert.Equal(strategy is Strategies.Deletion.SingleSoft or Strategies.Deletion.BulkSoft, deleted.Items.Exists(x => x.Id == content.Id));
    }

    [Theory]
    [InlineData(Strategies.Deletion.SingleSoft)]
    [InlineData(Strategies.Deletion.SinglePermanent)]
    [InlineData(Strategies.Deletion.BulkSoft)]
    [InlineData(Strategies.Deletion.BulkPermanent)]
    public async Task Should_create_content_with_custom_id_and_delete_it(Strategies.Deletion strategy)
    {
        var id = $"custom-{Guid.NewGuid()}";

        // STEP 1: Create a new item with a custom id.
        var options = new ContentCreateOptions { Id = id, Publish = true };

        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, options);

        Assert.Equal(id, content.Id);


        // STEP 2: Delete with selected strategy.
        await _.Client.DeleteAsync(content, strategy);


        // STEP 3: Retrieve all items and ensure that the deleted item does not exist.
        var updated = await _.Contents.GetAsync();

        Assert.DoesNotContain(updated.Items, x => x.Id == id);
    }

    [Theory]
    [InlineData(Strategies.Deletion.SingleSoft)]
    [InlineData(Strategies.Deletion.SinglePermanent)]
    [InlineData(Strategies.Deletion.BulkSoft)]
    [InlineData(Strategies.Deletion.BulkPermanent)]
    public async Task Should_recreate_deleted_content(Strategies.Deletion strategy)
    {
        // STEP 1: Create a new item.
        var content_1 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Delete with selected strategy.
        await _.Client.DeleteAsync(content_1, strategy);


        // STEP 3: Recreate the item with the same id.
        var createOptions = new ContentCreateOptions { Id = content_1.Id, Publish = true };

        var content_2 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, createOptions);

        Assert.Equal(Status.Published, content_2.Status);


        // STEP 4: Check if we can find it again with a query.
        var q = new ContentQuery { Filter = $"id eq '{content_1.Id}'" };

        var contents = await _.Contents.GetAsync(q);

        Assert.Contains(contents.Items, x => x.Id == content_2.Id);
    }

    [Theory]
    [InlineData(Strategies.Deletion.SingleSoft)]
    [InlineData(Strategies.Deletion.SinglePermanent)]
    [InlineData(Strategies.Deletion.BulkSoft)]
    [InlineData(Strategies.Deletion.BulkPermanent)]
    public async Task Should_recreate_deleted_content_with_upsert(Strategies.Deletion strategy)
    {
        // STEP 1: Create a new item.
        var content_1 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Delete with selected strategy.
        await _.Client.DeleteAsync(content_1, strategy);


        // STEP 3: Recreate the item with the same id.
        var content_2 = await _.Contents.UpsertAsync(content_1.Id, new TestEntityData
        {
            Number = 2
        }, ContentUpsertOptions.AsPublish);

        Assert.Equal(Status.Published, content_2.Status);
    }

    [Theory]
    [InlineData(Strategies.Deletion.SingleSoft)]
    [InlineData(Strategies.Deletion.SinglePermanent)]
    [InlineData(Strategies.Deletion.BulkSoft)]
    [InlineData(Strategies.Deletion.BulkPermanent)]
    public async Task Should_delete_recreated_content(Strategies.Deletion strategy)
    {
        var id = $"custom-{Guid.NewGuid()}";

        // STEP 1: Create a new item with a custom id.
        var options = new ContentCreateOptions { Id = id, Publish = true };

        var content_1 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, options);


        // STEP 2: Permanently delete content with custom id.
        await _.Contents.DeleteAsync(content_1.Id, new ContentDeleteOptions { Permanent = true });


        // STEP 3: Create a new item with same custom id.
        var content_2 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, options);

        Assert.Equal(2, content_2.Data.Number);


        // STEP 3: Permanently delete content with custom id again.
        await _.Client.DeleteAsync(content_2, strategy);
    }

    [Fact]
    public async Task Should_update_singleton_content_with_special_id()
    {
        var schemaName = $"schema-{Guid.NewGuid()}";

        // STEP 1: Create singleton.
        var createRequest = new CreateSchemaDto
        {
            Name = schemaName,
            IsPublished = true,
            IsSingleton = true,
            Fields = new List<UpsertSchemaFieldDto>
            {
                new UpsertSchemaFieldDto
                {
                    Name = "my-field",
                    Properties = new StringFieldPropertiesDto()
                }
            }
        };

        await _.Client.Schemas.PostSchemaAsync(createRequest);


        var client = _.Client.DynamicContents(schemaName);

        // STEP 2: Get content.
        var content_1 = await client.GetAsync("_schemaId_");

        Assert.NotNull(content_1);


        // STEP 3: Update content.
        var content_2 = await client.UpdateAsync("_schemaId_", new DynamicData
        {
            ["my-field"] = new JObject
            {
                ["iv"] = "singleton"
            }
        });

        Assert.Equal("singleton", content_2.Data["my-field"]["iv"]);

        await Verify(content_2);
    }

    [Fact]
    public async Task Should_get_content_by_version()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 1
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Update content.
        content = await _.Contents.UpdateAsync(content.Id, new TestEntityData
        {
            Number = 2
        });


        // STEP 3: Get current version.
        var content_latest = await _.Contents.GetAsync(content.Id);

        Assert.Equal(2, content_latest.Data.Number);


        // STEP 4: Get current version.
        var content_2 = await _.Contents.GetAsync(content.Id, content.Version);

        Assert.Equal(2, content_2.Data.Number);


        // STEP 4: Get previous version.
        var content_1 = await _.Contents.GetAsync(content.Id, content.Version - 1);

        Assert.Equal(1, content_1.Data.Number);

        await Verify(content_1);
    }

    [Fact]
    public async Task Should_update_content_in_parallel()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 3: Make parallel updates.
        await Parallel.ForEachAsync(Enumerable.Range(0, 20), async (i, ct) =>
        {
            try
            {
                await _.Contents.UpdateAsync(content.Id, new TestEntityData
                {
                    Number = i
                });
            }
            catch (SquidexException ex) when (ex.StatusCode is 409 or 412)
            {
                return;
            }
        });


        // STEP 3: Make an normal update to ensure nothing is corrupt.
        await _.Contents.UpdateAsync(content.Id, new TestEntityData
        {
            Number = 2
        });

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal(2, content.Data.Number);
    }

    [Fact]
    public async Task Should_upsert_content_in_parallel()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 3: Make parallel upserts.
        await Parallel.ForEachAsync(Enumerable.Range(0, 20), async (i, ct) =>
        {
            try
            {
                await _.Contents.UpsertAsync(content.Id, new TestEntityData
                {
                    Number = i
                });
            }
            catch (SquidexException ex) when (ex.StatusCode is 409 or 412)
            {
                return;
            }
        });


        // STEP 3: Make an normal update to ensure nothing is corrupt.
        await _.Contents.UpdateAsync(content.Id, new TestEntityData
        {
            Number = 2
        });

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal(2, content.Data.Number);
    }
}
