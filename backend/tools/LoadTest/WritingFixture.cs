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

namespace LoadTest
{
    public sealed class WritingFixture : IDisposable
    {
        public SquidexClient<TestEntity, TestEntityData> Client { get; private set; }

        public WritingFixture()
        {
            Task.Run(async () =>
            {
                Client = await TestClient.BuildAsync("reading");
            }).Wait();
        }

        public void Dispose()
        {
        }
    }
}
