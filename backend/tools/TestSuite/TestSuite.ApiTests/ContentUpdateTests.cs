// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
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
        TestEntity content = null;
        try
        {
            // STEP 1: Create the item unpublished.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_not_return_archived_content()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create the item published.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_not_return_unpublished_content()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create the item unpublished.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_strange_text()
    {
        const string text = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

        TestEntity content = null;
        try
        {
            // STEP 1: Create a content item with a text that caused a bug before.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = text
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Get the item and ensure that the text is the same.
            var queried = await _.Contents.GetAsync(content.Id);

            Assert.Equal(text, queried.Data.String);

            await Verify(queried);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_null_text()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a content item with a text that caused a bug before.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_json_with_dot()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a content item with a text that caused a bug before.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_default_text()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a content item with a text that caused a bug before.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                Localized = new Dictionary<string, string>()
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Get the item and ensure that the text is the same.
            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal("default", updated.Data.Localized["en"]);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_non_published_content()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create the item unpublished.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_published_content()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create the item published.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                Number = 1
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Get the item.
            await _.Contents.GetAsync(content.Id);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_content_with_custom_id()
    {
        var id = Guid.NewGuid().ToString();

        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item with a custom id.
            var options = new ContentCreateOptions { Id = id, Publish = true };

            content = await _.Contents.CreateAsync(new TestEntityData
            {
                Number = 1
            }, options);

            Assert.Equal(id, content.Id);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_not_create_content_with_custom_id_twice()
    {
        var id = Guid.NewGuid().ToString();

        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item with a custom id.
            var options = new ContentCreateOptions { Id = id, Publish = true };

            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_content_with_custom_id_and_upsert()
    {
        var id = Guid.NewGuid().ToString();

        TestEntity content = null;
        try
        {
            // STEP 1: Upsert a new item with a custom id.
            content = await _.Contents.UpsertAsync(id, new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_update_content()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                Number = 2
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Update the item and ensure that the data has changed.
            await _.Contents.UpdateAsync(content.Id, new TestEntityData
            {
                Number = 2
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal(2, content.Data.Number);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_update_content_in_parallel()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_upsert_content_in_parallel()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_update_content_to_null()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = "initial"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Update the item and ensure that the data has changed.
            await _.Contents.UpdateAsync(content.Id, new TestEntityData
            {
                String = null
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Null(updated.Data.String);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_patch_content()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = "test"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Patch an item.
            await _.Contents.PatchAsync(content.Id, new TestEntityData
            {
                Number = 1
            });


            // STEP 3: Update the item and ensure that the data has changed.
            await _.Contents.PatchAsync(content.Id, new TestEntityData
            {
                Number = 2
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal(2, updated.Data.Number);

            // Should not change other value with patch.
            Assert.Equal("test", updated.Data.String);

            await Verify(updated);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_patch_id_data_value()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                Id = "id1"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Update the item and ensure that the data has changed.
            await _.Contents.PatchAsync(content.Id, new TestEntityData
            {
                Id = "id2"
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal("id2", updated.Data.Id);

            await Verify(updated);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_patch_content_to_null()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = "initial"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Update the item and ensure that the data has changed.
            await _.Contents.PatchAsync(content.Id, new
            {
                @string = new
                {
                    iv = (object)null
                }
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Null(updated.Data.String);

            await Verify(updated);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_patch_content_with_upsert()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = "test"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Patch an item.
            await _.Contents.UpsertAsync(content.Id, new TestEntityData
            {
                Number = 1
            }, ContentUpsertOptions.AsPatch);


            // STEP 3: Update the item and ensure that the data has changed.
            await _.Contents.UpsertAsync(content.Id, new TestEntityData
            {
                Number = 2
            }, ContentUpsertOptions.AsPatch);

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal(2, updated.Data.Number);

            // Should not change other value with patch.
            Assert.Equal("test", updated.Data.String);

            await Verify(updated);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_patch_content_with_bulk()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = "test"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Patch an item.
            await _.Contents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = content.Id,
                        Data = new
                        {
                            number = new
                            {
                                iv = 1
                            }
                        },
                        Patch = true
                    }
                }
            });


            // STEP 3: Update the item and ensure that the data has changed.
            await _.Contents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = content.Id,
                        Data = new
                        {
                            number = new
                            {
                                iv = 2
                            }
                        },
                        Patch = true
                    }
                }
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal(2, updated.Data.Number);

            // Should not change other value with patch.
            Assert.Equal("test", updated.Data.String);

            await Verify(updated);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_update_content_with_bulk_and_overriden_schema_name()
    {
        TestEntity content = null;
        try
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 0: Create dummy schema.
            var createSchema = new CreateSchemaDto
            {
                Name = schemaName,

                // Publish it to avoid validations issues.
                IsPublished = true
            };

            await _.Schemas.PostSchemaAsync(_.AppName, createSchema);



            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = "test"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Patch an item.
            var client = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);

            await client.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = content.Id,
                        Data = new
                        {
                            number = new
                            {
                                iv = 1
                            }
                        },
                        Schema = _.SchemaName
                    }
                }
            });


            // STEP 3: Update the item and ensure that the data has changed.
            await client.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = content.Id,
                        Data = new
                        {
                            number = new
                            {
                                iv = 2
                            }
                        },
                        Schema = _.SchemaName
                    }
                }
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal(2, updated.Data.Number);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_update_content_with_bulk_and_shared_client()
    {
        TestEntity content = null;
        try
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 0: Create dummy schema.
            var createSchema = new CreateSchemaDto
            {
                Name = schemaName,

                // Publish it to avoid validations issues.
                IsPublished = true
            };

            await _.Schemas.PostSchemaAsync(_.AppName, createSchema);



            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
            {
                String = "test"
            }, ContentCreateOptions.AsPublish);


            // STEP 2: Patch an item.
            await _.SharedContents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = content.Id,
                        Data = new
                        {
                            number = new
                            {
                                iv = 1
                            }
                        },
                        Schema = _.SchemaName
                    }
                }
            });


            // STEP 3: Update the item and ensure that the data has changed.
            await _.SharedContents.BulkUpdateAsync(new BulkUpdate
            {
                Jobs = new List<BulkUpdateJob>
                {
                    new BulkUpdateJob
                    {
                        Id = content.Id,
                        Data = new
                        {
                            number = new
                            {
                                iv = 2
                            }
                        },
                        Schema = _.SchemaName
                    }
                }
            });

            var updated = await _.Contents.GetAsync(content.Id);

            Assert.Equal(2, updated.Data.Number);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Fact]
    public async Task Should_create_draft_version()
    {
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
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


            // STEP 4: Get the unpublished version
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
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_delete_content(bool permanent)
    {
        // STEP 1: Create a new item.
        var content_1 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Delete the item.
        await _.Contents.DeleteAsync(content_1.Id, new ContentDeleteOptions { Permanent = permanent });


        // STEP 3: Retrieve all items and ensure that the deleted item does not exist.
        var updated = await _.Contents.GetAsync();

        Assert.DoesNotContain(updated.Items, x => x.Id == content_1.Id);


        // STEP 4: Retrieve all deleted items and check if found.
        var q = new ContentQuery { Filter = "isDeleted eq true" };

        var deleted = await _.Contents.GetAsync(q, QueryContext.Default.Unpublished(true));

        Assert.Equal(!permanent, deleted.Items.Any(x => x.Id == content_1.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_recreate_deleted_content(bool permanent)
    {
        // STEP 1: Create a new item.
        var content_1 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Delete the item.
        var createOptions = new ContentDeleteOptions { Permanent = permanent };

        await _.Contents.DeleteAsync(content_1.Id, createOptions);


        // STEP 3: Recreate the item with the same id.
        var deleteOptions = new ContentCreateOptions { Id = content_1.Id, Publish = true };

        var content_2 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, deleteOptions);

        Assert.Equal(Status.Published, content_2.Status);


        // STEP 4: Check if we can find it again with a query.
        var q = new ContentQuery { Filter = $"id eq '{content_1.Id}'" };

        var contents_4 = await _.Contents.GetAsync(q);

        Assert.NotNull(contents_4.Items.Find(x => x.Id == content_1.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_recreate_deleted_content_with_upsert(bool permanent)
    {
        // STEP 1: Create a new item.
        var content_1 = await _.Contents.CreateAsync(new TestEntityData
        {
            Number = 2
        }, ContentCreateOptions.AsPublish);


        // STEP 2: Delete the item.
        var deleteOptions = new ContentDeleteOptions { Permanent = permanent };

        await _.Contents.DeleteAsync(content_1.Id, deleteOptions);


        // STEP 3: Recreate the item with the same id.
        var content_2 = await _.Contents.UpsertAsync(content_1.Id, new TestEntityData
        {
            Number = 2
        }, ContentUpsertOptions.AsPublish);

        Assert.Equal(Status.Published, content_2.Status);


        // STEP 4: Check if we can find it again with a query.
        var q = new ContentQuery { Filter = $"id eq '{content_1.Id}'" };

        var contents_4 = await _.Contents.GetAsync(q);

        Assert.NotNull(contents_4.Items.Find(x => x.Id == content_1.Id));
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

        await _.Schemas.PostSchemaAsync(_.AppName, createRequest);


        var client = _.ClientManager.CreateDynamicContentsClient(schemaName);

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
        TestEntity content = null;
        try
        {
            // STEP 1: Create a new item.
            content = await _.Contents.CreateAsync(new TestEntityData
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
            var data_2 = await _.Contents.GetDataAsync(content.Id, content.Version);

            Assert.Equal(2, data_2.Number);


            // STEP 4: Get previous version
            var data_1 = await _.Contents.GetDataAsync(content.Id, content.Version - 1);

            Assert.Equal(1, data_1.Number);

            await Verify(data_1);
        }
        finally
        {
            if (content != null)
            {
                await _.Contents.DeleteAsync(content.Id);
            }
        }
    }
}
