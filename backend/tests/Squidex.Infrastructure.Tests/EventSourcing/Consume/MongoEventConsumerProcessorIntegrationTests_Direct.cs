// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.EventSourcing.Consume;

[Trait("Category", "Dependencies")]
public class MongoEventConsumerProcessorIntegrationTests_Direct : EventConsumerProcessorIntegrationTests, IClassFixture<MongoEventStoreDirectFixture>
{
    public MongoEventStoreFixture _ { get; }

    public MongoEventConsumerProcessorIntegrationTests_Direct(MongoEventStoreDirectFixture fixture)
    {
        _ = fixture;
    }

    public override IEventStore CreateStore()
    {
        return _.EventStore;
    }
}
