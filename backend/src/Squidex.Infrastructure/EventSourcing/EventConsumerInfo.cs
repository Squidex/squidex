// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans.Concurrency;

namespace Squidex.Infrastructure.EventSourcing
{
    [Immutable]
    public sealed class EventConsumerInfo
    {
        public bool IsStopped { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }
    }
}
