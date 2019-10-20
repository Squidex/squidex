// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents
{
    public sealed class EnrichedManualEvent : EnrichedEvent
    {
        public override long Partition
        {
            get { return 0; }
        }
    }
}
