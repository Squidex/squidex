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
        private static readonly HashSet<string> CreatedSchemas = new HashSet<string>();

        public IContentsClient<TestEntity, TestEntityData> Contents { get; }

        public string SchemaName { get; }

        public ContentFixture()
            : this("my-writes")
        {
        }

        protected ContentFixture(string schemaName)
        {
            SchemaName = schemaName;

            if (!CreatedSchemas.Contains(schemaName))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await TestEntity.CreateSchemaAsync(Schemas, AppName, schemaName);
                    }
                    catch (SquidexManagementException ex)
                    {
                        if (ex.StatusCode != 400)
                        {
                            throw;
                        }
                    }
                }).Wait();

                CreatedSchemas.Add(schemaName);
            }

            Contents = ClientManager.CreateContentsClient<TestEntity, TestEntityData>(SchemaName);
        }
    }
}
