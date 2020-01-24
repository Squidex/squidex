// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using ApiTest.Model;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

namespace ApiTest.Fixtures
{
    public class ContentFixture : CreatedAppFixture
    {
        public SquidexClient<TestEntity, TestEntityData> Contents { get; }

        public string SchemaName { get; set; } = "my-schema";

        public string FieldNumber { get; } = "number";

        public string FieldString { get; } = "string";

        public ContentFixture()
        {
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
            });

            Contents = ClientManager.GetClient<TestEntity, TestEntityData>(SchemaName);
        }
    }
}
