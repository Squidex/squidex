// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

namespace LoadTest
{
    public sealed class ClientQueryFixture : IDisposable
    {
        public SquidexClient<TestEntity, TestEntityData> Client { get; } = TestClient.Build();

        public ClientQueryFixture()
        {
            Task.Run(async () =>
            {
                var apps = TestClient.ClientManager.CreateAppsClient();

                try
                {
                    await apps.PostAppAsync(new CreateAppDto
                    {
                        Name = TestClient.TestAppName
                    });

                    var schemas = TestClient.ClientManager.CreateSchemasClient();

                    await schemas.PostSchemaAsync(TestClient.TestAppName, new CreateSchemaDto
                    {
                        Name = TestClient.TestSchemaName,
                        Fields = new List<UpsertSchemaFieldDto>
                        {
                            new UpsertSchemaFieldDto
                            {
                                Name = TestClient.TestSchemaField,
                                Properties = new NumberFieldPropertiesDto()
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

                var contents = await Client.GetAllAsync();

                foreach (var content in contents.Items)
                {
                    await Client.DeleteAsync(content);
                }

                for (var i = 10; i > 0; i--)
                {
                    await Client.CreateAsync(new TestEntityData { Value = i }, true);
                }
            }).Wait();
        }

        public void Dispose()
        {
        }
    }
}
