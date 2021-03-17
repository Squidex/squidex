// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Azure.Documents.Client;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed class CosmosDbEventStoreFixture : IDisposable
    {
        private const string EmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string EmulatorUri = "https://localhost:8081";
        private readonly DocumentClient client;

        public CosmosDbEventStore EventStore { get; }

        public CosmosDbEventStoreFixture()
        {
            client = new DocumentClient(new Uri(EmulatorUri), EmulatorKey, TestUtils.DefaultSettings());

            EventStore = new CosmosDbEventStore(client, EmulatorKey, "Test", TestUtils.DefaultSerializer);
            EventStore.InitializeAsync().Wait();
        }

        public void Dispose()
        {
            client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri("Test")).Wait();
            client.Dispose();
        }
    }
}
