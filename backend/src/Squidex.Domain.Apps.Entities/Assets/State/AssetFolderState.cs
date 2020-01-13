﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Assets.State
{
    public sealed class AssetFolderState : DomainObjectState<AssetFolderState>, IAssetFolderEntity
    {
        [DataMember]
        public NamedId<Guid> AppId { get; set; }

        [DataMember]
        public string FolderName { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public Guid ParentId { get; set; }

        public override bool ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case AssetFolderCreated e:
                    {
                        SimpleMapper.Map(e, this);

                        return true;
                    }

                case AssetFolderRenamed e when e.FolderName != FolderName:
                    {
                        FolderName = e.FolderName;

                        return true;
                    }

                case AssetFolderMoved e when e.ParentId != ParentId:
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
