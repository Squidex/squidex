// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using LoadTest.Model;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;

namespace LoadTest
{
    public sealed class ReadingFixture : IDisposable
    {
        public SquidexClient<TestEntity, TestEntityData> Client { get; private set; }

        public IAppsClient AppsClient { get; private set; }

        public ReadingFixture()
        {
            Task.Run(async () =>
            {
                Client = await TestClient.BuildAsync("reading");

                var contents = await Client.GetAllAsync();

                if (contents.Total != 10)
                {
                    foreach (var content in contents.Items)
                    {
                        await Client.DeleteAsync(content);
                    }

                    for (var i = 10; i > 0; i--)
                    {
                        await Client.CreateAsync(new TestEntityData { Value = i }, true);
                    }
                }

                AppsClient = TestClient.ClientManager.CreateAppsClient();
            }).Wait();
        }

        public void Dispose()
        {
        }
    }
}
