// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });


                // STEP 2: Publish the item.
                await _.Contents.ChangeStatusAsync(content.Id, Status.Published);


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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, true);


                // STEP 2: Archive the item.
                await _.Contents.ChangeStatusAsync(content.Id, Status.Archived);


                // STEP 3. Get a 404 for the item because it is not published anymore.
                await Assert.ThrowsAsync<SquidexException>(() => _.Contents.GetAsync(content.Id));
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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });


                // STEP 2: Change the status to publiushed and then to draft.
                await _.Contents.ChangeStatusAsync(content.Id, Status.Published);
                await _.Contents.ChangeStatusAsync(content.Id, Status.Draft);


                // STEP 3. Get a 404 for the item because it is not published anymore.
                await Assert.ThrowsAsync<SquidexException>(() => _.Contents.GetAsync(content.Id));
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
                content = await _.Contents.CreateAsync(new TestEntityData { String = text }, true);


                // STEP 2: Get the item and ensure that the text is the same.
                var updated = await _.Contents.GetAsync(content.Id);

                Assert.Equal(text, updated.Data.String);
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
                }, true);


                // STEP 2: Get the item and ensure that the text is the same.
                var updated = await _.Contents.GetAsync(content.Id, QueryContext.Default.IgnoreFallback());

                Assert.Null(updated.Data.Localized["en"]);
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
                }, true);


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
        public async Task Should_create_content_with_scripting()
        {
            TestEntity content = null;
            try
            {
                // STEP 1: Create a content item with a value that triggers the schema.
                content = await _.Contents.CreateAsync(new TestEntityData { Number = -99 }, true);

                Assert.True(content.Data.Number > 0);
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
        public async Task Should_create_bulk_content_with_scripting()
        {
            TestEntity content = null;
            try
            {
                // STEP 1: Create content with a value that triggers the schema.
                var results = await _.Contents.BulkUpdateAsync(new BulkUpdate
                {
                    DoNotScript = false,
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Upsert,
                            Data = new
                            {
                                number = new
                                {
                                    iv = TestEntity.ScriptTrigger
                                }
                            }
                        }
                    },
                    Publish = true
                });


                // STEP 2: Query content.
                content = await _.Contents.GetAsync(results[0].ContentId);

                Assert.True(content.Data.Number > 0);
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
        public async Task Should_create_bulk_content_with_scripting_but_disabled()
        {
            TestEntity content = null;
            try
            {
                // STEP 1: Create content with a value that triggers the schema.
                var results = await _.Contents.BulkUpdateAsync(new BulkUpdate
                {
                    Jobs = new List<BulkUpdateJob>
                    {
                        new BulkUpdateJob
                        {
                            Type = BulkUpdateType.Upsert,
                            Data = new
                            {
                                number = new
                                {
                                    iv = TestEntity.ScriptTrigger
                                }
                            }
                        }
                    },
                    Publish = true
                });


                // STEP 2: Query content.
                content = await _.Contents.GetAsync(results[0].ContentId);

                Assert.Equal(-99, content.Data.Number);
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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 });


                // STEP 2. Get a 404 for the item because it is not published.
                await Assert.ThrowsAsync<SquidexException>(() => _.Contents.GetAsync(content.Id));
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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, true);


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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, id, true);

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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, id, true);

                Assert.Equal(id, content.Id);


                // STEP 2: Create a new item with a custom id.
                var ex = await Assert.ThrowsAsync<SquidexException>(() => _.Contents.CreateAsync(new TestEntityData { Number = 1 }, id, true));

                Assert.Contains("\"statusCode\":409", ex.Message);
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
                content = await _.Contents.UpsertAsync(id, new TestEntityData { Number = 1 }, true);

                Assert.Equal(id, content.Id);


                // STEP 2: Make an update with the upsert endpoint.
                content = await _.Contents.UpsertAsync(id, new TestEntityData { Number = 2 });

                Assert.Equal(2, content.Data.Number);


                // STEP 3: Make an update with the update endpoint.
                content = await _.Contents.UpdateAsync(id, new TestEntityData { Number = 3 });

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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 2 }, true);


                // STEP 2: Update the item and ensure that the data has changed.
                await _.Contents.UpdateAsync(content.Id, new TestEntityData { Number = 2 });

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
                content = await _.Contents.CreateAsync(new TestEntityData { String = "initial" }, true);


                // STEP 2: Update the item and ensure that the data has changed.
                await _.Contents.UpdateAsync(content.Id, new TestEntityData { String = null });

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
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, true);


                // STEP 2: Update the item and ensure that the data has changed.
                await _.Contents.PatchAsync(content.Id, new TestEntityData { Number = 2 });

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
        public async Task Should_patch_content_to_null()
        {
            TestEntity content = null;
            try
            {
                // STEP 1: Create a new item.
                content = await _.Contents.CreateAsync(new TestEntityData { String = "initial" }, true);


                // STEP 2: Update the item and ensure that the data has changed.
                await _.Contents.PatchAsync(content.Id, new { @string = new { iv = (object)null } });

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
        public async Task Should_create_draft_version()
        {
            TestEntity content = null;
            try
            {
                // STEP 1: Create a new item.
                content = await _.Contents.CreateAsync(new TestEntityData { Number = 1 }, true);


                // STEP 2: Create draft.
                content = await _.Contents.CreateDraftAsync(content.Id);


                // STEP 3: Update the item and ensure that the data has not changed.
                await _.Contents.PatchAsync(content.Id, new TestEntityData { Number = 2 });

                var updated_1 = await _.Contents.GetAsync(content.Id);

                Assert.Equal(1, updated_1.Data.Number);


                // STEP 4: Get the unpublished version
                var unpublished = await _.Contents.GetAsync(content.Id, QueryContext.Default.Unpublished());

                Assert.Equal(2, unpublished.Data.Number);


                // STEP 5: Publish draft and ensure that it has been updated.
                await _.Contents.ChangeStatusAsync(content.Id, "Published");

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
            var content_1 = await _.Contents.CreateAsync(new TestEntityData { Number = 2 }, true);


            // STEP 2: Delete the item.
            await _.Contents.DeleteAsync(content_1.Id, permanent);


            // STEP 3: Retrieve all items and ensure that the deleted item does not exist.
            var updated = await _.Contents.GetAsync();

            Assert.DoesNotContain(updated.Items, x => x.Id == content_1.Id);


            // STEP 4: Retrieve all deleted items and check if found.
            var deleted = await _.Contents.GetAsync(new ContentQuery
            {
                Filter = "isDeleted eq true"
            }, QueryContext.Default.Unpublished(true));

            Assert.Equal(!permanent, deleted.Items.Any(x => x.Id == content_1.Id));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_recreate_deleted_content(bool permanent)
        {
            // STEP 1: Create a new item.
            var content_1 = await _.Contents.CreateAsync(new TestEntityData { Number = 2 }, true);


            // STEP 2: Delete the item.
            await _.Contents.DeleteAsync(content_1.Id, permanent);


            // STEP 3: Recreate the item with the same id.
            var content_2 = await _.Contents.CreateAsync(new TestEntityData { Number = 2 }, content_1.Id, true);

            Assert.Equal(Status.Published, content_2.Status);


            // STEP 4: Check if we can find it again with a query.
            var contents_4 = await _.Contents.GetAsync(new ContentQuery { Filter = $"id eq '{content_1.Id}'" });

            Assert.NotNull(contents_4.Items.FirstOrDefault(x => x.Id == content_1.Id));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_recreate_deleted_content_with_upsert(bool permanent)
        {
            // STEP 1: Create a new item.
            var content_1 = await _.Contents.CreateAsync(new TestEntityData { Number = 2 }, true);


            // STEP 2: Delete the item.
            await _.Contents.DeleteAsync(content_1.Id, permanent);


            // STEP 3: Recreate the item with the same id.
            var content_2 = await _.Contents.UpsertAsync(content_1.Id, new TestEntityData { Number = 2 }, true);

            Assert.Equal(Status.Published, content_2.Status);


            // STEP 4: Check if we can find it again with a query.
            var contents_4 = await _.Contents.GetAsync(new ContentQuery { Filter = $"id eq '{content_1.Id}'" });

            Assert.NotNull(contents_4.Items.FirstOrDefault(x => x.Id == content_1.Id));
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
        }
    }
}
