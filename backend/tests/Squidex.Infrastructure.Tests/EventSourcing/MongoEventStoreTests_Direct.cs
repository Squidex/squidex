﻿// ==========================================================================
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
    public class MongoEventStoreTests_Direct : EventStoreTests<MongoEventStore>, IClassFixture<MongoEventStoreDirectFixture>
    {
        public MongoEventStoreFixture _ { get; }

        protected override int SubscriptionDelayInMs { get; } = 1000;

        public MongoEventStoreTests_Direct(MongoEventStoreDirectFixture fixture)
        {
            _ = fixture;
        }

        public override MongoEventStore CreateStore()
        {
            return _.EventStore;
        }
    }
}
