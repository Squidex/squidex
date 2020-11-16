// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.State
{
    public sealed class AssetFolderState : DomainObjectState<AssetFolderState>, IAssetFolderEntity
    {
        public NamedId<DomainId> AppId { get; set; }

        public string FolderName { get; set; }

        public DomainId ParentId { get; set; }

        [IgnoreDataMember]
        public DomainId UniqueId
        {
            get { return DomainId.Combine(AppId, Id); }
        }

        public override bool ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case AssetFolderCreated e:
                    {
                        Id = e.AssetFolderId;

                        SimpleMapper.Map(e, this);

                        return true;
                    }

                case AssetFolderRenamed e when Is.OptionalChange(FolderName, e.FolderName):
                    {
                        FolderName = e.FolderName;

                        return true;
                    }

                case AssetFolderMoved e when Is.Change(ParentId, e.ParentId):
                    {
                        ParentId = e.ParentId;

                        return true;
                    }

                case AssetFolderDeleted _:
                    {
                        IsDeleted = true;

                        return true;
                    }
            }

            return false;
        }
    }
}
