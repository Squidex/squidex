// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Squidex.Infrastructure.EventSourcing
{
    [Trait("Dependency", "MongoDB")]
    public class MongoEventStoreTests : EventStoreTests<MongoEventStore>, IClassFixture<MongoEventStoreFixture>
    {
        private readonly MongoEventStoreFixture fixture;

        protected override int SubscriptionDelayInMs { get; } = 1000;

        public MongoEventStoreTests(MongoEventStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override MongoEventStore CreateStore()
        {
            return fixture.EventStore;
        }
    }
}
