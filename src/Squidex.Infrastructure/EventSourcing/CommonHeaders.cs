// ==========================================================================
//  CommonHeaders.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public static class CommonHeaders
    {
        public static readonly string AggregateId = "AggregateId";

        public static readonly string CommitId = "CommitId";

        public static readonly string EventId = "EventId";

        public static readonly string EventNumber = "EventNumber";

        public static readonly string EventStreamNumber = "EventStreamNumber";

        public static readonly string SnapshotVersion = "SnapshotVersion";

        public static readonly string Timestamp = "Timestamp";

        public static readonly string Actor = "Actor";
    }
}
