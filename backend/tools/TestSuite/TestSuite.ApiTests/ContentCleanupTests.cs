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
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using TestSuite.Model;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class ContentCleanupTests : IClassFixture<ClientFixture>
    {
        public ClientFixture _ { get; }

        public ContentCleanupTests(ClientFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_cleanup_old_data_from_update_response()
        {
            var schemaName = $"schema-{DateTime.UtcNow.Ticks}";

            // STEP 1: Create a schema.
            var schema = await _.Schemas.PostSchemaAsync(_.AppName, new CreateSchemaDto
            {
                Name = schemaName,
                Fields = new List<UpsertSchemaFieldDto>
                {
                    new UpsertSchemaFieldDto
                    {
                        Name = "number",
                        Properties = new NumberFieldPropertiesDto
                        {
                            IsRequired = true
                        }
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = "string",
                        Properties = new StringFieldPropertiesDto
                        {
                            IsRequired = false
                        }
                    }
                },
                IsPublished = true
            });

            var contents = _.ClientManager.GetClient<TestEntity, TestEntityData>(schemaName);

            // STEP 2: Create a content for this schema.
            var data = new TestEntityData { Number = 12, String = "hello" };

            var content_1 = await contents.CreateAsync(data);

            Assert.Equal(data.String, content_1.DataDraft.String);


            // STEP 3: Delete a field from schema.
            await _.Schemas.DeleteFieldAsync(_.AppName, schema.Name, schema.Fields.ElementAt(1).FieldId);


            // STEP 4: Make any update.
            var content_2 = await contents.ChangeStatusAsync(content_1.Id, "Published");

            // Should not return deleted field.
            Assert.Null(content_2.DataDraft.String);
        }
    }
}
