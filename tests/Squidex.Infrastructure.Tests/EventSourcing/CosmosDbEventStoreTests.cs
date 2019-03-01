// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    [Trait("Category", "Dependencies")]
    public class CosmosDbEventStoreTests : EventStoreTests<CosmosDbEventStore>, IClassFixture<CosmosDbEventStoreFixture>
    {
        private readonly CosmosDbEventStoreFixture fixture;

        protected override int SubscriptionDelayInMs { get; } = 1000;

        public CosmosDbEventStoreTests(CosmosDbEventStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override CosmosDbEventStore CreateStore()
        {
            return fixture.EventStore;
        }
    }
}
