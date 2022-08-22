// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Subscriptions
{
    public sealed class ContentSubscription
    {
        public DomainId AppId { get; set; }

        public DomainId SchemaId { get; set; }

        public EnrichedContentEventType? Type { get; set; }

        public ValueTask<bool> ShouldHandle(object message)
        {
            return new ValueTask<bool>(ShouldHandleCore(message));
        }

        private bool ShouldHandleCore(object message)
        {
            if (message is not EnrichedContentEvent @event)
            {
                return false;
            }

            return SchemaId == @event.SchemaId.Id && (Type == null || @event.Type == Type.Value);
        }
    }
}
