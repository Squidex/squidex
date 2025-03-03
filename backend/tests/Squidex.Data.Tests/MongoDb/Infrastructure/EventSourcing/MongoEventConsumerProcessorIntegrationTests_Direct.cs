// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Events;

namespace Squidex.MongoDb.Infrastructure.EventSourcing;

[Trait("Category", "Dependencies")]
public class MongoEventConsumerProcessorIntegrationTests_Direct(MongoEventStoreFixture_Direct fixture) : EventConsumerProcessorIntegrationTests, IClassFixture<MongoEventStoreFixture_Direct>
{
    public override IEventStore CreateStore()
    {
        return fixture.EventStore;
    }
}
