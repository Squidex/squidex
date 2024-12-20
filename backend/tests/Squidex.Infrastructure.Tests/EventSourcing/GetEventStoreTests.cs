﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Infrastructure.EventSourcing;

[Trait("Category", "Dependencies")]
public class GetEventStoreTests(GetEventStoreFixture fixture) : EventStoreTests<GetEventStore>, IClassFixture<GetEventStoreFixture>
{
    public GetEventStoreFixture _ { get; } = fixture;

    public override GetEventStore CreateStore()
    {
        return _.EventStore;
    }
}
