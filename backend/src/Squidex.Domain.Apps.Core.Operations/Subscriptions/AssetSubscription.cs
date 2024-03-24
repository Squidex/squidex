// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Domain.Apps.Core.Subscriptions;

public sealed class AssetSubscription : ISubscription
{
    public EnrichedAssetEventType? Type { get; init; }

    public ValueTask<bool> ShouldHandle(object message)
    {
        return new ValueTask<bool>(ShouldHandleCore(message));
    }

    private bool ShouldHandleCore(object message)
    {
        switch (message)
        {
            case EnrichedAssetEvent enrichedAssetEvent:
                return CheckType(enrichedAssetEvent);
            case AssetEvent assetEvent:
                return CheckType(assetEvent);
            default:
                return false;
        }
    }

    private bool CheckType(EnrichedAssetEvent @event)
    {
        return Type == null || Type.Value == @event.Type;
    }

    private bool CheckType(AssetEvent @event)
    {
        switch (Type)
        {
            case EnrichedAssetEventType.Created:
                return @event is AssetCreated;
            case EnrichedAssetEventType.Deleted:
                return @event is AssetDeleted;
            case EnrichedAssetEventType.Annotated:
                return @event is AssetAnnotated;
            case EnrichedAssetEventType.Updated:
                return @event is AssetUpdated;
            default:
                return true;
        }
    }
}
