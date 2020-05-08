// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.ClientLibrary;
using TestSuite.Model;

namespace TestSuite.Fixtures
{
    public sealed class ContentReferencesFixture : CreatedAppFixture
    {
        public string SchemaName { get; } = "references";

        public IContentsClient<TestEntityWithReferences, TestEntityWithReferencesData> Contents { get; }

        public ContentReferencesFixture()
        {
            Task.Run(async () =>
            {
                await TestEntityWithReferences.CreateSchemaAsync(Schemas, AppName, SchemaName);
            }).Wait();

            Contents = ClientManager.CreateContentsClient<TestEntityWithReferences, TestEntityWithReferencesData>(SchemaName);
        }
    }
}
