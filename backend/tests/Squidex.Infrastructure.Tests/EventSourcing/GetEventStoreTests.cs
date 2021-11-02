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
    public class GetEventStoreTests : EventStoreTests<GetEventStore>, IClassFixture<GetEventStoreFixture>
    {
        public GetEventStoreFixture _ { get; }

        protected override int SubscriptionDelayInMs { get; } = 1000;

        public GetEventStoreTests(GetEventStoreFixture fixture)
        {
            _ = fixture;
        }

        public override GetEventStore CreateStore()
        {
            return _.EventStore;
        }
    }
}
