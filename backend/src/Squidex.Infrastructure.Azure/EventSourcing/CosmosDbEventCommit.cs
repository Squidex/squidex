// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
{
    internal sealed class CosmosDbEventCommit
    {
        public Guid Id { get; set; }

        public CosmosDbEvent[] Events { get; set; }

        public long EventStreamOffset { get; set; }

        public long EventsCount { get; set; }

        public string EventStream { get; set; }

        public long Timestamp { get; set; }
    }
}
