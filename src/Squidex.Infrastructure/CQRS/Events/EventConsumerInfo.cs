// ==========================================================================
//  EventConsumerInfo.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events
{
    public sealed class EventConsumerInfo
    {
        public bool IsStopped { get; set; }

        public string Name { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }
    }
}
