// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Model;

namespace TestSuite.Fixtures
{
    public class ContentFixture : CreatedAppFixture
    {
        public SquidexClient<TestEntity, TestEntityData> Contents { get; }

        public string SchemaName { get; }

        public string FieldNumber { get; } = "number";

        public string FieldString { get; } = "string";

        public ContentFixture()
            : this("my-writes")
        {
        }

        protected ContentFixture(string schemaName)
        {
            SchemaName = schemaName;

            Task.Run(async () =>
            {
                try
                {
                    var schemas = ClientManager.CreateSchemasClient();

                    await schemas.PostSchemaAsync(TestClient.TestAppName, new CreateSchemaDto
                    {
                        Name = SchemaName,
                        Fields = new List<UpsertSchemaFieldDto>
                        {
                            new UpsertSchemaFieldDto
                            {
                                Name = FieldNumber,
                                Properties = new NumberFieldPropertiesDto
                                {
                                    IsRequired = true
                                }
                            },
                            new UpsertSchemaFieldDto
                            {
                                Name = FieldString,
                                Properties = new StringFieldPropertiesDto
                                {
                                    IsRequired = false
                                }
                            }
                        },
                        IsPublished = true
                    });
                }
                catch (SquidexManagementException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }
            }).Wait();

            Contents = ClientManager.GetClient<TestEntity, TestEntityData>(SchemaName);
        }
    }
}
