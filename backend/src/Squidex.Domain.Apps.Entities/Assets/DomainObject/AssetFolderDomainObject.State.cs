// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public sealed partial class AssetFolderDomainObject
{
    protected override AssetFolder Apply(AssetFolder snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        switch (@event.Payload)
        {
            case AssetFolderCreated e:
                newSnapshot = new AssetFolder { Id = e.AssetFolderId };
                SimpleMapper.Map(e, newSnapshot);
                break;

            case AssetFolderRenamed e:
                newSnapshot = snapshot.Rename(e.FolderName);
                break;

            case AssetFolderMoved e:
                newSnapshot = snapshot.Move(e.ParentId);
                break;

            case AssetFolderDeleted:
                newSnapshot = snapshot with { IsDeleted = true };
                break;
        }

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}
