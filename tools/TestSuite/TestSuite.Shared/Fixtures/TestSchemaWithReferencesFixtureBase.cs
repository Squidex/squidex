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

public abstract class TestSchemaWithReferencesFixtureBase : CreatedAppFixture
{
    public IContentsClient<TestEntityWithReferences, TestEntityWithReferencesData> Contents { get; private set; }

    public string SchemaName { get; }

    protected TestSchemaWithReferencesFixtureBase(string schemaName)
    {
        SchemaName = schemaName;
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await Factories.CreateAsync($"{nameof(TestEntityWithReferences)}_{SchemaName}", async () =>
        {
            try
            {
                await TestEntityWithReferences.CreateSchemaAsync(Schemas, AppName, SchemaName);
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

        Contents = ClientManager.CreateContentsClient<TestEntityWithReferences, TestEntityWithReferencesData>(SchemaName);
    }
}
