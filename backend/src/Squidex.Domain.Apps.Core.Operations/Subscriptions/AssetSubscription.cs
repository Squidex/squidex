// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events.Assets;

namespace Squidex.Domain.Apps.Core.Subscriptions
{
    public sealed class AssetSubscription : AppSubscription
    {
        public EnrichedAssetEventType? Type { get; set; }

        public override ValueTask<bool> ShouldHandle(object message)
        {
            return new ValueTask<bool>(ShouldHandleCore(message));
        }

        private bool ShouldHandleCore(object message)
        {
            if (message is EnrichedAssetEvent)
            {
                return true;
            }

            switch (Type)
            {
                case EnrichedAssetEventType.Created:
                    return message is AssetCreated;
                case EnrichedAssetEventType.Deleted:
                    return message is AssetDeleted;
                case EnrichedAssetEventType.Annotated:
                    return message is AssetAnnotated;
                case EnrichedAssetEventType.Updated:
                    return message is AssetUpdated;
                default:
                    return message is AssetEvent;
            }
        }
    }
}
