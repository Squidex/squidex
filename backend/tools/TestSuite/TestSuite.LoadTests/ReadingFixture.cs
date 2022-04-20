// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

namespace TestSuite.LoadTests
{
    public sealed class ReadingFixture : TestSchemaFixtureBase
    {
        public ReadingFixture()
            : base("benchmark-reading")
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await DisposeAsync();

            for (var i = 10; i > 0; i--)
            {
                var data = TestEntity.CreateTestEntry(i);

                await Contents.CreateAsync(data, ContentCreateOptions.AsPublish);
            }
        }

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();

            var contents = await Contents.GetAsync();

            foreach (var content in contents.Items)
            {
                await Contents.DeleteAsync(content);
            }
        }
    }
}
