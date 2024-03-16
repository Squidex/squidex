// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public sealed class EnrichedManualEvent : EnrichedUserEventBase
    {
        public override long Partition
        {
            get => 0;
        }
    }
}
