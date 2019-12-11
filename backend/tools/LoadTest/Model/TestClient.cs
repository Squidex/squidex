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

namespace LoadTest.Model
{
    public static class TestClient
    {
        public const string ServerUrl = "http://localhost:5000";

        public const string ClientId = "root";
        public const string ClientSecret = "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=";

        public const string TestAppName = "integration-tests";

        public static readonly SquidexClientManager ClientManager =
            new SquidexClientManager(
                ServerUrl,
                TestAppName,
                ClientId,
                ClientSecret)
            {
                ReadResponseAsString = true
            };

        public static async Task<SquidexClient<TestEntity, TestEntityData>> BuildAsync(string schemaName)
        {
            await CreateAppIfNotExistsAsync();
            await CreateSchemaIfNotExistsAsync(schemaName);

            return ClientManager.GetClient<TestEntity, TestEntityData>(schemaName);
        }

        private static async Task CreateAppIfNotExistsAsync()
        {
            try
            {
                var apps = ClientManager.CreateAppsClient();

                await apps.PostAppAsync(new CreateAppDto
                {
                    Name = TestAppName
                });
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }
        }

        private static async Task CreateSchemaIfNotExistsAsync(string schemaName)
        {
            try
            {
                var schemas = ClientManager.CreateSchemasClient();

                await schemas.PostSchemaAsync(TestAppName, new CreateSchemaDto
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
            }
            catch (SquidexManagementException ex)
            {
                if (ex.StatusCode != 400)
                {
                    throw;
                }
            }
        }
    }
}