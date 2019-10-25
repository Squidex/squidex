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
    public class GetEventStoreTests : EventStoreTests<GetEventStore>, IClassFixture<GetEventStoreFixture>
    {
        private readonly GetEventStoreFixture fixture;

        protected override int SubscriptionDelayInMs { get; } = 1000;

        public GetEventStoreTests(GetEventStoreFixture fixture)
        {
            this.fixture = fixture;
        }

        public override GetEventStore CreateStore()
        {
            return fixture.EventStore;
        }
    }
}
