// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.EnrichedEvents;
using TestSuite.Model;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

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
            Localized = new Dictionary<string, string?>
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

        Assert.Equal(42, (int)queried.Data.Json!["field.with.dot"]!);

        await Verify(queried);
    }

    [Fact]
    public async Task Should_create_default_text()
    {
        // STEP 1: Create a content item with a text that caused a bug before.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Localized = []
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
    [InlineData("e5f7c399-6024-4367-9538-b7de4bced0f9' UNION ALL SELECT NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL-- vIGQ")]
    [InlineData("e5f7c399-6024-4367-9538-b7de4bced0f9') UNION ALL SELECT 59,59,59,'qkjvq'||'FWUHnOHbQqCmewsQoKRPxqUXmHZMTtqHmyYoRhwu'||'qvpkq',59-- dLuxn")]
    public async Task Should_create_and_delete_content_with_strange_id(string id)
    {
        // STEP 1: Upsert a new item with a custom id.
        var content_0 = await _.Contents.UpsertAsync(id, new TestEntityData(), ContentUpsertOptions.AsPublish);

        Assert.Equal(id, content_0.Id);

        // STEP 2: Find content
        var content_1 = await _.Contents.GetAsync(id);

        Assert.Equal(id, content_1.Id);


        // STEP 3: Delete content.
        await _.Contents.DeleteAsync(id);

        await Assert.ThrowsAnyAsync<SquidexException>(() => _.Contents.GetAsync(id));
    }

    [Theory]
    [InlineData(ContentStrategies.Update.Normal)]
    [InlineData(ContentStrategies.Update.Upsert)]
    [InlineData(ContentStrategies.Update.UpsertBulk)]
    [InlineData(ContentStrategies.Update.Bulk)]
    [InlineData(ContentStrategies.Update.BulkShared)]
    [InlineData(ContentStrategies.Update.BulkWithSchema)]
    public async Task Should_update_content(ContentStrategies.Update strategy)
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

    [Fact]
    public async Task Should_update_content_with_script()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 100
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Update content with script.
        await _.Client.DynamicContents(_.SchemaName).UpdateAsync(content.Id,
            new DynamicData
            {
                [TestEntityData.NumberField] = new JObject
                {
                    ["iv"] = new JObject
                    {
                        ["$update"] = "$data.number.iv + 42"
                    }
                }
            });

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal(142, updated.Data.Number);

        // Other data fields are overwritten.
        Assert.Null(updated.Data.String);
    }

    [Fact]
    public async Task Should_unset_field_with_patch()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = "Hello",
            // Not relevant for the test, but required in the schema.
            Number = 100
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Update content with script.
        await _.Client.DynamicContents(_.SchemaName).PatchAsync(content.Id,
            new DynamicData
            {
                [TestEntityData.StringField] = new JObject
                {
                    ["$unset"] = true
                }
            });

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Null(updated.Data.String);
    }

    [Fact]
    public async Task Should_unset_field_value_with_patch()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = "Hello",
            // Not relevant for the test, but required in the schema.
            Number = 100
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Update content with script.
        await _.Client.DynamicContents(_.SchemaName).PatchAsync(content.Id,
            new DynamicData
            {
                [TestEntityData.StringField] = new JObject
                {
                    ["iv"] = new JObject
                    {
                        ["$unset"] = true
                    }
                }
            });

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Null(updated.Data.String);
    }

    [Theory]
    [InlineData(ContentStrategies.Update.Normal)]
    [InlineData(ContentStrategies.Update.Upsert)]
    [InlineData(ContentStrategies.Update.UpsertBulk)]
    [InlineData(ContentStrategies.Update.Bulk)]
    [InlineData(ContentStrategies.Update.BulkShared)]
    [InlineData(ContentStrategies.Update.BulkWithSchema)]
    public async Task Should_update_content_to_null(ContentStrategies.Update strategy)
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
    [InlineData(ContentStrategies.Patch.Normal)]
    [InlineData(ContentStrategies.Patch.Upsert)]
    [InlineData(ContentStrategies.Patch.UpsertBulk)]
    [InlineData(ContentStrategies.Patch.Bulk)]
    [InlineData(ContentStrategies.Patch.BulkShared)]
    [InlineData(ContentStrategies.Patch.BulkWithSchema)]
    public async Task Should_patch_content(ContentStrategies.Patch strategy)
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

    [Fact]
    public async Task Should_patch_content_with_script()
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 100
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Patch content with script.
        await _.Client.DynamicContents(_.SchemaName).PatchAsync(content.Id,
            new DynamicData
            {
                [TestEntityData.NumberField] = new JObject
                {
                    ["iv"] = new JObject
                    {
                        ["$update"] = "$data.number.iv + 42"
                    }
                }
            });

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Equal(142, updated.Data.Number);

        // Other data fields are overwritten.
        Assert.Null(updated.Data.String);
    }

    [Theory]
    [InlineData(ContentStrategies.Patch.Normal)]
    [InlineData(ContentStrategies.Patch.Upsert)]
    [InlineData(ContentStrategies.Patch.UpsertBulk)]
    [InlineData(ContentStrategies.Patch.Bulk)]
    [InlineData(ContentStrategies.Patch.BulkShared)]
    [InlineData(ContentStrategies.Patch.BulkWithSchema)]
    public async Task Should_patch_id_data_value(ContentStrategies.Patch strategy)
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
    [InlineData(ContentStrategies.Patch.Normal)]
    [InlineData(ContentStrategies.Patch.Upsert)]
    [InlineData(ContentStrategies.Patch.UpsertBulk)]
    [InlineData(ContentStrategies.Patch.Bulk)]
    [InlineData(ContentStrategies.Patch.BulkShared)]
    [InlineData(ContentStrategies.Patch.BulkWithSchema)]
    public async Task Should_patch_content_to_null(ContentStrategies.Patch strategy)
    {
        // STEP 1: Create a new item.
        var content = await _.Contents.CreateAsync(new TestEntityData
        {
            String = "initial"
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Patch with selected strategy.
        await _.Client.PatchAsync(content, new
        {
            @string = new { iv = (string?)null }
        }, strategy);

        var updated = await _.Contents.GetAsync(content.Id);

        Assert.Null(updated.Data.String);
    }

    [Theory]
    [InlineData(ContentStrategies.Deletion.SingleSoft)]
    [InlineData(ContentStrategies.Deletion.SinglePermanent)]
    [InlineData(ContentStrategies.Deletion.BulkSoft)]
    [InlineData(ContentStrategies.Deletion.BulkPermanent)]
    public async Task Should_delete_content(ContentStrategies.Deletion strategy)
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

        Assert.Equal(strategy is ContentStrategies.Deletion.SingleSoft or ContentStrategies.Deletion.BulkSoft, deleted.Items.Exists(x => x.Id == content.Id));
    }

    [Theory]
    [InlineData(ContentStrategies.Deletion.SingleSoft)]
    [InlineData(ContentStrategies.Deletion.SinglePermanent)]
    [InlineData(ContentStrategies.Deletion.BulkSoft)]
    [InlineData(ContentStrategies.Deletion.BulkPermanent)]
    public async Task Should_create_content_with_custom_id_and_delete_it(ContentStrategies.Deletion strategy)
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
    [InlineData(ContentStrategies.Deletion.SingleSoft)]
    [InlineData(ContentStrategies.Deletion.SinglePermanent)]
    [InlineData(ContentStrategies.Deletion.BulkSoft)]
    [InlineData(ContentStrategies.Deletion.BulkPermanent)]
    public async Task Should_recreate_deleted_content(ContentStrategies.Deletion strategy)
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
    [InlineData(ContentStrategies.Deletion.SingleSoft)]
    [InlineData(ContentStrategies.Deletion.SinglePermanent)]
    [InlineData(ContentStrategies.Deletion.BulkSoft)]
    [InlineData(ContentStrategies.Deletion.BulkPermanent)]
    public async Task Should_recreate_deleted_content_with_upsert(ContentStrategies.Deletion strategy)
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
    [InlineData(ContentStrategies.Deletion.SingleSoft)]
    [InlineData(ContentStrategies.Deletion.SinglePermanent)]
    [InlineData(ContentStrategies.Deletion.BulkSoft)]
    [InlineData(ContentStrategies.Deletion.BulkPermanent)]
    public async Task Should_delete_recreated_content(ContentStrategies.Deletion strategy)
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
            Fields =
            [
                new UpsertSchemaFieldDto
                {
                    Name = "my-field",
                    Properties = new StringFieldPropertiesDto()
                },
            ]
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
                }, ct: ct);
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
                }, ct: ct);
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

    [Theory]
    [InlineData(ContentStrategies.EnrichDefaults.Normal)]
    [InlineData(ContentStrategies.EnrichDefaults.Update)]
    [InlineData(ContentStrategies.EnrichDefaults.UpdateBulk)]
    [InlineData(ContentStrategies.EnrichDefaults.Upsert)]
    [InlineData(ContentStrategies.EnrichDefaults.UpsertBulk)]
    [InlineData(ContentStrategies.EnrichDefaults.Bulk)]
    [InlineData(ContentStrategies.EnrichDefaults.BulkWithSchema)]
    [InlineData(ContentStrategies.EnrichDefaults.BulkShared)]
    public async Task Should_enrich_default_fields(ContentStrategies.EnrichDefaults strategy)
    {
        var schemaName = $"schema-{Guid.NewGuid()}";

        // STEP 0: Create initial schema.
        var schemaRequest = new CreateSchemaDto
        {
            Name = schemaName,
            IsPublished = true
        };

        await _.Client.Schemas.PostSchemaAsync(schemaRequest);

        var contents = _.Client.DynamicContents(schemaName);


        // STEP 1: Create a new item.
        var content_0 = await contents.CreateAsync([], ContentCreateOptions.AsPublish);

        Assert.Null(content_0.Data.GetValueOrDefault("fieldDefault"));


        // STEP 2: Add new fields.
        var fieldRequest = new AddFieldDto
        {
            Name = "fieldDefault",
            Properties = new StringFieldPropertiesDto
            {
                DefaultValue = "Hello Squidex",
                IsRequired = false
            }
        };

        await _.Client.Schemas.PostFieldAsync(schemaName, fieldRequest);


        // STEP 3: Create required field.
        await _.Client.EnrichDefaultsAsync(content_0, content_0.Data, strategy, false);


        // STEP 4: Get content.
        var context = QueryContext.Default.WithHeaderHandler(request =>
        {
            request.Headers.TryAddWithoutValidation("X-NoDefaults", "1");
        });

        var content_1 = await contents.GetAsync(content_0.Id, context);

        Assert.Equal("Hello Squidex", content_1.Data["fieldDefault"]!["iv"]!.ToString());
    }

    [Theory]
    [InlineData(ContentStrategies.EnrichDefaults.Normal)]
    [InlineData(ContentStrategies.EnrichDefaults.Bulk)]
    [InlineData(ContentStrategies.EnrichDefaults.BulkWithSchema)]
    [InlineData(ContentStrategies.EnrichDefaults.BulkShared)]
    public async Task Should_enrich_non_required_default_fields(ContentStrategies.EnrichDefaults strategy)
    {
        var schemaName = $"schema-{Guid.NewGuid()}";

        // STEP 0: Create initial schema.
        var schemaRequest = new CreateSchemaDto
        {
            Name = schemaName,
            IsPublished = true
        };

        await _.Client.Schemas.PostSchemaAsync(schemaRequest);

        var contents = _.Client.DynamicContents(schemaName);


        // STEP 1: Create a new item.
        var content_0 = await contents.CreateAsync([], ContentCreateOptions.AsPublish);

        Assert.Null(content_0.Data.GetValueOrDefault("fieldDefault"));
        Assert.Null(content_0.Data.GetValueOrDefault("fieldRequired"));


        // STEP 2: Add new fields.
        var fieldRequest1 = new AddFieldDto
        {
            Name = "fieldDefault",
            Properties = new StringFieldPropertiesDto
            {
                DefaultValue = "Hello Squidex",
                IsRequired = false
            }
        };

        var fieldRequest2 = new AddFieldDto
        {
            Name = "fieldRequired",
            Properties = new StringFieldPropertiesDto
            {
                DefaultValue = "Hello Required",
                IsRequired = true
            }
        };

        await _.Client.Schemas.PostFieldAsync(schemaName, fieldRequest1);
        await _.Client.Schemas.PostFieldAsync(schemaName, fieldRequest2);


        // STEP 3: Create required field.
        await _.Client.EnrichDefaultsAsync(content_0, content_0.Data, strategy, false);


        // STEP 4: Get content.
        var context = QueryContext.Default.WithHeaderHandler(request =>
        {
            request.Headers.TryAddWithoutValidation("X-NoDefaults", "1");
        });

        var content_1 = await contents.GetAsync(content_0.Id, context);

        Assert.Equal("Hello Squidex", content_1.Data["fieldDefault"]!["iv"]!.ToString());
        Assert.Null(content_0.Data.GetValueOrDefault("fieldRequired"));
    }

    [Theory]
    [InlineData(ContentStrategies.EnrichDefaults.Normal)]
    [InlineData(ContentStrategies.EnrichDefaults.Bulk)]
    [InlineData(ContentStrategies.EnrichDefaults.BulkWithSchema)]
    [InlineData(ContentStrategies.EnrichDefaults.BulkShared)]
    public async Task Should_enrich_required_default_field_if_flag_is_true(ContentStrategies.EnrichDefaults strategy)
    {
        var schemaName = $"schema-{Guid.NewGuid()}";

        // STEP 0: Create initial schema.
        var schemaRequest = new CreateSchemaDto
        {
            Name = schemaName,
            IsPublished = true
        };

        await _.Client.Schemas.PostSchemaAsync(schemaRequest);

        var contents = _.Client.DynamicContents(schemaName);


        // STEP 1: Create a new item.
        var content_0 = await contents.CreateAsync([], ContentCreateOptions.AsPublish);

        Assert.Null(content_0.Data.GetValueOrDefault("fieldDefault"));
        Assert.Null(content_0.Data.GetValueOrDefault("fieldRequired"));


        // STEP 2: Add new field.
        var fieldRequest = new AddFieldDto
        {
            Name = "fieldRequired",
            Properties = new StringFieldPropertiesDto
            {
                DefaultValue = "Hello Required",
                IsRequired = true
            }
        };
        await _.Client.Schemas.PostFieldAsync(schemaName, fieldRequest);


        // STEP 3: Create required field.
        await _.Client.EnrichDefaultsAsync(content_0, content_0.Data, strategy, true);


        // STEP 4: Get content.
        var context = QueryContext.Default.WithHeaderHandler(request =>
        {
            request.Headers.TryAddWithoutValidation("X-NoDefaults", "1");
        });

        var content_1 = await contents.GetAsync(content_0.Id, context);

        Assert.Equal("Hello Required", content_1.Data["fieldRequired"]!["iv"]!.ToString());
    }

    [Fact]
    public async Task Should_update_as_bulk()
    {
        var prefix = Guid.NewGuid().ToString();

        // STEP 1: Create contents.
        var result_0 = await _.Contents.BulkUpdateAsync(new BulkUpdate
        {
            Jobs =
            [
                new BulkUpdateJob
                {
                    Data = new Dictionary<string, object>
                    {
                        [TestEntityData.StringField] = new
                        {
                            iv = $"{prefix}_1"
                        },
                        [TestEntityData.NumberField] = new
                        {
                            iv = 1
                        }
                    }
                },
                new BulkUpdateJob
                {
                    Data = new Dictionary<string, object>
                    {
                        [TestEntityData.StringField] = new
                        {
                            iv = $"{prefix}_2"
                        },
                        [TestEntityData.NumberField] = new
                        {
                            iv = 2
                        }
                    }
                },
            ],
            Publish = true
        });

        result_0 = result_0.OrderBy(x => x.JobIndex).ToList();


        // STEP 2: Update contents by filter.
        var result_1 = await _.Contents.BulkUpdateAsync(new BulkUpdate
        {
            Jobs =
            [
                new BulkUpdateJob
                {
                    Query = new
                    {
                        Filter = new
                        {
                            path = $"data.{TestEntityData.StringField}.iv",
                            op = "eq",
                            value = $"{prefix}_1"
                        }
                    },
                    Data = new Dictionary<string, object>
                    {
                        [TestEntityData.StringField] = new
                        {
                            iv = $"{prefix}_1_x"
                        }
                    },
                    Type = BulkUpdateType.Patch
                },
                new BulkUpdateJob
                {
                    Query = new
                    {
                        Filter = new
                        {
                            path = $"data.{TestEntityData.StringField}.iv",
                            op = "eq",
                            value = $"{prefix}_2"
                        }
                    },
                    Data = new Dictionary<string, object>
                    {
                        [TestEntityData.StringField] = new
                        {
                            iv = $"{prefix}_2_y"
                        }
                    },
                    Type = BulkUpdateType.Patch
                },
            ]
        });

        result_1.OrderBy(x => x.JobIndex).Should().BeEquivalentTo(new List<BulkResult>
        {
            new BulkResult
            {
                ContentId = result_0[0].ContentId,
                JobIndex = 0
            },
            new BulkResult
            {
                ContentId = result_0[1].ContentId,
                JobIndex = 1
            }
        });


        // STEP 2: Get contents.
        var contents = await _.Contents.GetAsync(new ContentQuery
        {
            Ids = result_0.Select(x => x.ContentId).ToHashSet()
        });

        var content0 = contents.Items.Find(x => x.Id == result_0[0].ContentId);
        var content1 = contents.Items.Find(x => x.Id == result_0[1].ContentId);

        Assert.Equal($"{prefix}_1_x", content0?.Data.String);
        Assert.Equal($"{prefix}_2_y", content1?.Data.String);
    }
}
