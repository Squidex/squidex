// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class ContentCleanupTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; }

        public ContentCleanupTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_cleanup_old_data_from_update_response()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create a schema.
            var schema = await TestEntity.CreateSchemaAsync(_.Schemas, _.AppName, schemaName);

            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);


            // STEP 2: Create a content for this schema.
            var data = new TestEntityData { Number = 12, String = "hello" };

            var content_1 = await contents.CreateAsync(data);

            Assert.Equal(data.String, content_1.Data.String);


            // STEP 3: Delete a field from schema.
            await _.Schemas.DeleteFieldAsync(_.AppName, schema.Name, schema.Fields.ElementAt(1).FieldId);


            // STEP 4: Make any update.
            var content_2 = await contents.ChangeStatusAsync(content_1.Id, "Published");

            // Should not return deleted field.
            Assert.Null(content_2.Data.String);
        }

        [Fact]
        public async Task Should_cleanup_old_references()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create a schema.
            await TestEntityWithReferences.CreateSchemaAsync(_.Schemas, _.AppName, schemaName);

            var contents = _.ClientManager.CreateContentsClient<TestEntityWithReferences, TestEntityWithReferencesData>(schemaName);


            // STEP 2: Create a referenced content.
            var dataA = new TestEntityWithReferencesData();

            var contentA_1 = await contents.CreateAsync(dataA);


            // STEP 3: Create a content with a reference.
            var dataB = new TestEntityWithReferencesData { References = new[] { contentA_1.Id } };

            var contentB_1 = await contents.CreateAsync(dataB);


            // STEP 3: Delete a reference
            await contents.DeleteAsync(contentA_1.Id);


            // STEP 4: Make any update.
            var contentB_2 = await contents.ChangeStatusAsync(contentB_1.Id, "Published");

            // Should not return deleted field.
            Assert.Empty(contentB_2.Data.References);
        }
    }
}
