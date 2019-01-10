// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public sealed class EnrichedUsageExceededEvent : EnrichedEvent
    {
        public long Current { get; set; }

        public long Limit { get; set; }

        public override long Partition
        {
            get { return AppId.GetHashCode(); }
        }
    }
}
