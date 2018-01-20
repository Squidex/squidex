// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public static readonly string Timestamp = "Timestamp";
    }
}
