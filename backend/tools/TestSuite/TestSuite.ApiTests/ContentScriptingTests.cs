// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
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
        public async Task Should_use_creating_and_query_tests()
        {
            var schemaName = $"schema-{Guid.NewGuid()}";

            // STEP 1: Create a schema.
            await TestEntity.CreateSchemaAsync(_.Schemas, _.AppName, schemaName);


            // STEP 2: Set scripts
            await _.Schemas.PutScriptsAsync(_.AppName, schemaName, new SchemaScriptsDto
            {
                Query = @$"
                    ctx.data.{TestEntityData.StringField}.iv = ctx.data.{TestEntityData.StringField}.iv + '_Updated'

                    replace()",
                Create = @$"
                    ctx.data.{TestEntityData.NumberField}.iv *= 2;
                    replace()"
            });


            // STEP 3: Create content
            var contents = _.ClientManager.CreateContentsClient<TestEntity, TestEntityData>(schemaName);

            var data = new TestEntityData { Number = 13, String = "Hello" };

            var content = await contents.CreateAsync(data);

            Assert.Equal(26, content.Data.Number);
            Assert.Equal("Hello_Updated", content.Data.String);
        }
    }
}
