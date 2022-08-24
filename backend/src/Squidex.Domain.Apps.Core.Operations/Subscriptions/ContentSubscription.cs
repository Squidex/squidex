// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events.Contents;

namespace Squidex.Domain.Apps.Core.Subscriptions
{
    public sealed class ContentSubscription : AppSubscription
    {
        public string? SchemaName { get; set; }

        public EnrichedContentEventType? Type { get; set; }

        public override ValueTask<bool> ShouldHandle(object message)
        {
            return new ValueTask<bool>(ShouldHandleCore(message));
        }

        private bool ShouldHandleCore(object message)
        {
            if (message is EnrichedContentEvent)
            {
                return true;
            }

            if (message is not ContentEvent @event)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SchemaName) && @event.SchemaId.Name != SchemaName)
            {
                return false;
            }

            switch (Type)
            {
                case EnrichedContentEventType.Created:
                    return @event is ContentCreated;
                case EnrichedContentEventType.Deleted:
                    return @event is ContentDeleted;
                case EnrichedContentEventType.Published:
                    return @event is ContentStatusChanged { Change: Contents.StatusChange.Published };
                case EnrichedContentEventType.Unpublished:
                    return @event is ContentStatusChanged { Change: Contents.StatusChange.Unpublished };
                case EnrichedContentEventType.StatusChanged:
                    return @event is ContentStatusChanged { Change: Contents.StatusChange.Change };
                case EnrichedContentEventType.Updated:
                    return @event is ContentUpdated;
                default:
                    return true;
            }
        }
    }
}
