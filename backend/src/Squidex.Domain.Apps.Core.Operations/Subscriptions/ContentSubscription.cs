// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Subscriptions;

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
        switch (message)
        {
            case EnrichedContentEvent enrichedContentEvent:
                return ShouldHandle(enrichedContentEvent);
            case ContentEvent contentEvent:
                return ShouldHandle(contentEvent);
            default:
                return false;
        }
    }

    private bool ShouldHandle(EnrichedContentEvent @event)
    {
        var schemaName = @event.SchemaId.Name;

        return CheckSchema(schemaName) && CheckType(@event) && CheckPermission(@event.AppId.Name, schemaName);
    }

    private bool ShouldHandle(ContentEvent @event)
    {
        var schemaName = @event.SchemaId.Name;

        return CheckSchema(schemaName) && CheckType(@event) && CheckPermission(@event.AppId.Name, schemaName);
    }

    private bool CheckSchema(string schemaName)
    {
        return string.IsNullOrWhiteSpace(SchemaName) || schemaName == SchemaName;
    }

    private bool CheckType(EnrichedContentEvent @event)
    {
        return Type == null || Type.Value == @event.Type;
    }

    private bool CheckType(ContentEvent @event)
    {
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

    private bool CheckPermission(string appName, string schemaName)
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppContentsRead, appName, schemaName);

        return Permissions.Allows(permission);
    }
}
