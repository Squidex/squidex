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

public partial class AssetDomainObject
{
    protected override Asset Apply(Asset snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        switch (@event.Payload)
        {
            case AssetCreated e:
                newSnapshot = new Asset { Id = e.AssetId, TotalSize = e.FileSize };
                SimpleMapper.Map(e, newSnapshot);
                break;

            case AssetUpdated e when !string.Equals(e.FileHash, snapshot.FileHash, StringComparison.Ordinal):
                newSnapshot = snapshot with { TotalSize = snapshot.FileSize + e.FileSize };
                SimpleMapper.Map(e, newSnapshot);
                break;

            case AssetAnnotated e:
                newSnapshot = snapshot.Annotate(e.FileName, e.Slug, e.IsProtected, e.Tags, e.Metadata);
                break;

            case AssetMoved e:
                newSnapshot = snapshot.Move(e.ParentId);
                break;

            case AssetDeleted:
                newSnapshot = snapshot with { IsDeleted = true };
                break;
        }

        if (newSnapshot.Tags == null)
        {
            newSnapshot = newSnapshot with { Tags = [] };
        }

        if (newSnapshot.Metadata == null)
        {
            newSnapshot = newSnapshot with { Metadata = [] };
        }

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}
