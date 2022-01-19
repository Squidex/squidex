﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class ContentScriptingTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; }

        public ContentScriptingTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_create_content_with_scripting()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            var scripts = new SchemaScriptsDto
            {
                Create = @$"
                    ctx.data.{TestEntityData.NumberField}.iv *= 2;
                    replace()"
            };

            // STEP 1: Create a schema.
            await TestEntity.CreateSchemaAsync(_.Schemas, _.AppName, schemaName, scripts);


            // STEP 2: Create content
            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);

            var content = await contents.CreateAsync(new TestEntityData { Number = 13 });

            Assert.Equal(26, content.Data.Number);
        }

        [Fact]
        public async Task Should_query_content_with_scripting()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            var scripts = new SchemaScriptsDto
            {
                Query = @$"
                    ctx.data.{TestEntityData.NumberField}.iv *= 2;
                    replace()",
            };

            // STEP 1: Create a schema.
            await TestEntity.CreateSchemaAsync(_.Schemas, _.AppName, schemaName, scripts);


            // STEP 2: Create content
            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);

            var content_0 = await contents.CreateAsync(new TestEntityData { Number = 13 }, ContentCreateOptions.AsPublish);


            // STEP 2: Query content
            var content_1 = await contents.GetAsync(content_0.Id);

            Assert.Equal(26, content_1.Data.Number);
        }

        [Fact]
        public async Task Should_query_content_with_scripting_and_pre_query()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            var scripts = new SchemaScriptsDto
            {
                QueryPre = @$"
                    ctx.test = 17",
                Query = @$"
                    ctx.data.{TestEntityData.NumberField}.iv = ctx.test + 2;
                    replace()",
            };

            // STEP 1: Create a schema.
            await TestEntity.CreateSchemaAsync(_.Schemas, _.AppName, schemaName, scripts);


            // STEP 2: Create content
            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);

            var content_0 = await contents.CreateAsync(new TestEntityData { Number = 99 }, ContentCreateOptions.AsPublish);


            // STEP 2: Query content
            var content_1 = await contents.GetAsync(content_0.Id);

            Assert.Equal(26, content_1.Data.Number);
        }

        [Fact]
        public async Task Should_create_bulk_content_with_scripting()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create a schema.
            var scripts = new SchemaScriptsDto
            {
                Create = @$"
                    ctx.data.{TestEntityData.NumberField}.iv = incremeentCounter('${schemaName}');
                    replace()"
            };

            await TestEntity.CreateSchemaAsync(_.Schemas, _.AppName, schemaName, scripts);


            // STEP 2: Create content with a value that triggers the schema.
            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);

            var results = await contents.BulkUpdateAsync(new BulkUpdate
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
                                iv = 99
                            }
                        }
                    }
                },
                Publish = true
            });

            Assert.Single(results);
            Assert.Null(results[0].Error);


            // STEP 2: Query content.
            var content = await contents.GetAsync(results[0].ContentId);

            Assert.True(content.Data.Number > 0);
        }

        [Fact]
        public async Task Should_create_bulk_content_with_scripting_but_disabled()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create a schema.
            var scripts = new SchemaScriptsDto
            {
                Create = @$"
                    ctx.data.{TestEntityData.NumberField}.iv = incremeentCounter('${schemaName}');
                    replace()"
            };

            await TestEntity.CreateSchemaAsync(_.Schemas, _.AppName, schemaName, scripts);


            // STEP 1: Create content with a value that triggers the schema.
            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);

            var results = await contents.BulkUpdateAsync(new BulkUpdate
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
                                iv = 99
                            }
                        }
                    }
                },
                Publish = true
            });

            Assert.Single(results);
            Assert.Null(results[0].Error);


            // STEP 2: Query content.
            var content = await contents.GetAsync(results[0].ContentId);

            Assert.Equal(-99, content.Data.Number);
        }
    }
}
