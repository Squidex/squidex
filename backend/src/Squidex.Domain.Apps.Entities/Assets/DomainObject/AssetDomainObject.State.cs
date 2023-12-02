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

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject;

public partial class AssetDomainObject
{
    protected override Asset Apply(Asset snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        switch (@event.Payload)
        {
            case AssetCreated e:
                newSnapshot = snapshot with
                {
                    Id = e.AssetId,
                    AppId = e.AppId,
                    FileHash = e.FileHash,
                    FileName = e.FileName,
                    FileSize = e.FileSize,
                    FileVersion = 0,
                    Metadata = e.Metadata ?? [],
                    MimeType = e.MimeType,
                    ParentId = e.ParentId,
                    Tags = e.Tags ?? [],
                    TotalSize = snapshot.FileSize + e.FileSize,
                    Type = e.Type
                };
                break;

            case AssetUpdated e when !string.Equals(e.FileHash, snapshot.FileHash):
                newSnapshot = snapshot with
                {
                    FileHash = e.FileHash,
                    FileSize = e.FileSize,
                    FileVersion = e.FileVersion,
                    Metadata = e.Metadata ?? [],
                    MimeType = e.MimeType,
                    Tags = snapshot.Tags ?? [],
                    TotalSize = snapshot.FileSize + e.FileSize,
                    Type = e.Type
                };
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

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}
