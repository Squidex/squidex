// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Assets.State
{
    public class AssetState : DomainObjectState<AssetState>, IAssetItemEntity
    {
        [DataMember]
        public NamedId<Guid> AppId { get; set; }

        [DataMember]
        public string FolderName { get; set; }

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
        public bool IsImage { get; set; }

        [DataMember]
        public int? PixelWidth { get; set; }

        [DataMember]
        public int? PixelHeight { get; set; }

        [DataMember]
        public bool IsFolder { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public Guid ParentId { get; set; }

        [DataMember]
        public HashSet<string> Tags { get; set; }

        public void ApplyEvent(IEvent @event)
        {
            switch (@event)
            {
                case AssetFolderCreated e:
                    {
                        SimpleMapper.Map(e, this);

                        break;
                    }

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

                        break;
                    }

                case AssetUpdated e:
                    {
                        SimpleMapper.Map(e, this);

                        TotalSize += e.FileSize;

                        break;
                    }

                case AssetAnnotated e:
                    {
                        if (!string.IsNullOrWhiteSpace(e.FileName))
                        {
                            FileName = e.FileName;
                        }

                        if (!string.IsNullOrWhiteSpace(e.Slug))
                        {
                            Slug = e.Slug;
                        }

                        if (e.Tags != null)
                        {
                            Tags = e.Tags;
                        }

                        break;
                    }

                case AssetFolderRenamed e:
                    {
                        SimpleMapper.Map(e, this);

                        break;
                    }

                case AssetItemMoved e:
                    {
                        ParentId = e.ParentId;

                        break;
                    }

                case AssetItemDeleted _:
                    {
                        IsDeleted = true;

                        break;
                    }
            }
        }

        public override AssetState Apply(Envelope<IEvent> @event)
        {
            return Clone().Update(@event, (e, s) => s.ApplyEvent(e));
        }
    }
}
