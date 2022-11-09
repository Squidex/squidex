// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Subscriptions;

public sealed class AssetSubscription : AppSubscription
{
    public EnrichedAssetEventType? Type { get; set; }

    public override ValueTask<bool> ShouldHandle(object message)
    {
        return new ValueTask<bool>(ShouldHandleCore(message));
    }

    private bool ShouldHandleCore(object message)
    {
        switch (message)
        {
            case EnrichedAssetEvent enrichedAssetEvent:
                return ShouldHandle(enrichedAssetEvent);
            case AssetEvent assetEvent:
                return ShouldHandle(assetEvent);
            default:
                return false;
        }
    }

    private bool ShouldHandle(EnrichedAssetEvent @event)
    {
        return CheckType(@event) && CheckPermission(@event.AppId.Name);
    }

    private bool ShouldHandle(AssetEvent @event)
    {
        return CheckType(@event) && CheckPermission(@event.AppId.Name);
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

    private bool CheckPermission(string appName)
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppAssetsRead, appName);

        return Permissions.Includes(permission);
    }
}
