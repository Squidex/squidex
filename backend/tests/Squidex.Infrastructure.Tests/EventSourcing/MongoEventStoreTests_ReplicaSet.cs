// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.EventSourcing
{
    [Trait("Category", "Dependencies")]
    public class MongoEventStoreTests_ReplicaSet : EventStoreTests<MongoEventStore>, IClassFixture<MongoEventStoreReplicaSetFixture>
    {
        public MongoEventStoreFixture _ { get; }

        protected override int SubscriptionDelayInMs { get; } = 1000;

        public MongoEventStoreTests_ReplicaSet(MongoEventStoreReplicaSetFixture fixture)
        {
            _ = fixture;
        }

        public override MongoEventStore CreateStore()
        {
            return _.EventStore;
        }
    }
}
