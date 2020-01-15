// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Assets.State
{
    public class AssetState : DomainObjectState<AssetState>, IAssetEntity
    {
        [DataMember]
        public NamedId<Guid> AppId { get; set; }

        [DataMember]
        public Guid ParentId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string FileHash { get; set; }

        [DataMember]
        public string MimeType { get; set; }

        [DataMember]
        public string Slug { get; set; }

        [DataMember]
        public long FileVersion { get; set; }

        [DataMember]
        public long FileSize { get; set; }

        [DataMember]
        public long TotalSize { get; set; }

        [DataMember]
        public HashSet<string> Tags { get; set; }

        [DataMember]
        public AssetMetadata Metadata { get; set; }

        [DataMember]
        public AssetType Type { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        public Guid AssetId
        {
            get { return Id; }
        }

        public override bool ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case AssetCreated e:
                    {
                        SimpleMapper.Map(e, this);

                        FileName = e.FileName;

                        if (string.IsNullOrWhiteSpace(e.Slug))
                        {
                            Slug = e.FileName.ToAssetSlug();
                        }
                        else
                        {
                            Slug = e.Slug;
                        }

                        TotalSize += e.FileSize;

                        return true;
                    }

                case AssetUpdated e:
                    {
                        SimpleMapper.Map(e, this);

                        TotalSize += e.FileSize;

                        return true;
                    }

                case AssetAnnotated e:
                    {
                        var hasChanged = false;

                        if (Is.Change(FileName, e.FileName))
                        {
                            FileName = e.FileName;

                            hasChanged = true;
                        }

                        if (Is.Change(Slug, e.Slug))
                        {
                            Slug = e.Slug;

                            hasChanged = true;
                        }

                        if (Is.Change(Tags, e.Tags))
                        {
                            Tags = e.Tags;

                            hasChanged = true;
                        }

                        if (Is.Change(Metadata, e.Metadata))
                        {
                            Metadata = e.Metadata;

                            hasChanged = true;
                        }

                        return hasChanged;
                    }

                case AssetMoved e when e.ParentId != ParentId:
                    {
                        ParentId = e.ParentId;

                        return true;
                    }

                case AssetDeleted _:
                    {
                        IsDeleted = true;

                        return true;
                    }
            }

            return false;
        }
    }
}
