// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Model;

namespace TestSuite.Fixtures
{
    public class ContentFixture : CreatedAppFixture
    {
        private static readonly HashSet<string> CreatedSchemas = new HashSet<string>();

        public IContentsClient<TestEntity, TestEntityData> Contents { get; private set; }

        public string SchemaName { get; }

        public ContentFixture()
            : this("my-writes")
        {
        }

        protected ContentFixture(string schemaName)
        {
            SchemaName = schemaName;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            if (CreatedSchemas.Add(SchemaName))
            {
                try
                {
                    await TestEntity.CreateSchemaAsync(Schemas, AppName, SchemaName);
                }
                catch (SquidexManagementException ex)
                {
                    if (ex.StatusCode != 400)
                    {
                        throw;
                    }
                }
            }

            Contents = ClientManager.CreateContentsClient<TestEntity, TestEntityData>(SchemaName);
        }
    }
}
