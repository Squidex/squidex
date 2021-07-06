// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class SchemaTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; }

        public SchemaTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_create_schema()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create schema
            var createRequest = new CreateSchemaDto { Name = schemaName };

            var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

            // Should return created schemas with correct name.
            Assert.Equal(schemaName, schema.Name);


            // STEP 2: Get all schemas
            var schemas = await _.Schemas.GetSchemasAsync(_.AppName);

            // Should provide new schema when apps are schemas.
            Assert.Contains(schemas.Items, x => x.Name == schemaName);
        }

        [Fact]
        public async Task Should_create_singleton_schema()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create schema
            var createRequest = new CreateSchemaDto
            {
                Name = schemaName,
                IsSingleton = true,
                IsPublished = true
            };

            var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

            // Should return created schemas with correct name.
            Assert.Equal(schemaName, schema.Name);


            // STEP 2: Get all schemas
            var schemas = await _.Schemas.GetSchemasAsync(_.AppName);

            // Should provide new schema when apps are schemas.
            Assert.Contains(schemas.Items, x => x.Name == schemaName);


            // STEP 3: Get singleton content
            var client = _.ClientManager.CreateDynamicContentsClient(schemaName);

            var content = await client.GetAsync(schema.Id);

            Assert.NotNull(content);
        }

        [Fact]
        public async Task Should_create_schema_with_checkboxes()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create schema
            var createRequest = new CreateSchemaDto
            {
                Name = schemaName,
                Fields = new List<UpsertSchemaFieldDto>
                {
                    new UpsertSchemaFieldDto
                    {
                        Name = "references",
                        Partitioning = "invariant",
                        Properties = new ReferencesFieldPropertiesDto
                        {
                            Editor = ReferencesFieldEditor.Checkboxes
                        }
                    },
                    new UpsertSchemaFieldDto
                    {
                        Name = "tags",
                        Partitioning = "invariant",
                        Properties = new TagsFieldPropertiesDto
                        {
                            Editor = TagsFieldEditor.Checkboxes,
                            AllowedValues = new List<string> { "value1" }
                        }
                    }
                }
            };

            var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

            // Should return created schemas with correct name.
            Assert.Equal(schemaName, schema.Name);
        }

        [Fact]
        public async Task Should_delete_Schema()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create schema
            var createRequest = new CreateSchemaDto { Name = schemaName };

            var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

            // Should return created schemas with correct name.
            Assert.Equal(schemaName, schema.Name);


            // STEP 2: Delete schema
            await _.Schemas.DeleteSchemaAsync(_.AppName, schemaName);

            var schemas = await _.Schemas.GetSchemasAsync(_.AppName);

            // Should not provide deleted schema when schema are queried.
            Assert.DoesNotContain(schemas.Items, x => x.Name == schemaName);
        }

        [Fact]
        public async Task Should_recreate_after_deleted()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create schema
            var createRequest = new CreateSchemaDto { Name = schemaName };

            var schema = await _.Schemas.PostSchemaAsync(_.AppName, createRequest);

            // Should return created schemas with correct name.
            Assert.Equal(schemaName, schema.Name);


            // STEP 2: Delete schema.
            await _.Schemas.DeleteSchemaAsync(_.AppName, schemaName);


            // STEP 3: Create app again
            await _.Schemas.PostSchemaAsync(_.AppName, createRequest);
        }
    }
}
