// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Model;

namespace TestSuite.Fixtures;

public abstract class TestSchemaFixtureBase : CreatedAppFixture
{
    public IContentsClient<TestEntity, TestEntityData> Contents { get; private set; }

    public string SchemaName { get; }

    protected TestSchemaFixtureBase(string schemaName)
    {
        SchemaName = schemaName;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await Factories.CreateAsync($"{nameof(TestEntity)}_{SchemaName}", async () =>
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

            return true;
        });

        Contents = ClientManager.CreateContentsClient<TestEntity, TestEntityData>(SchemaName);
    }
}
