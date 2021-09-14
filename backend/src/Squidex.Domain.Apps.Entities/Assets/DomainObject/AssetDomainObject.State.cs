// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject
{
    public sealed partial class AssetDomainObject
    {
        public sealed class State : DomainObjectState<State>, IAssetEntity
        {
            public NamedId<DomainId> AppId { get; set; }

            public DomainId ParentId { get; set; }

            public string FileName { get; set; }

            public string FileHash { get; set; }

            public string MimeType { get; set; }

            public string Slug { get; set; }

            public long FileVersion { get; set; }

            public long FileSize { get; set; }

            public long TotalSize { get; set; }

            public bool IsProtected { get; set; }

            public bool IsDeleted { get; set; }

            public HashSet<string> Tags { get; set; }

            public AssetMetadata Metadata { get; set; }

            public AssetType Type { get; set; }

            [IgnoreDataMember]
            public DomainId AssetId
            {
                get => Id;
            }

            [IgnoreDataMember]
            public DomainId UniqueId
            {
                get => DomainId.Combine(AppId, Id);
            }

            public override bool ApplyEvent(IEvent @event)
            {
                switch (@event)
                {
                    case AssetCreated e:
                        {
                            Id = e.AssetId;

                            SimpleMapper.Map(e, this);

                            if (string.IsNullOrWhiteSpace(Slug))
                            {
                                Slug = FileName.ToAssetSlug();
                            }

                            TotalSize += e.FileSize;

                            EnsureProperties();

                            return true;
                        }

                    case AssetUpdated e when Is.Change(e.FileHash, FileHash):
                        {
                            SimpleMapper.Map(e, this);

                            TotalSize += e.FileSize;

                            EnsureProperties();

                            return true;
                        }

                    case AssetAnnotated e:
                        {
                            var hasChanged = false;

                            if (Is.OptionalChange(FileName, e.FileName))
                            {
                                FileName = e.FileName;

                                hasChanged = true;
                            }

                            if (Is.OptionalChange(Slug, e.Slug))
                            {
                                Slug = e.Slug;

                                hasChanged = true;
                            }

                            if (Is.OptionalChange(IsProtected, e.IsProtected))
                            {
                                IsProtected = e.IsProtected.Value;

                                hasChanged = true;
                            }

                            if (Is.OptionalSetChange(Tags, e.Tags))
                            {
                                Tags = e.Tags;

                                hasChanged = true;
                            }

                            if (Is.OptionalMapChange(Metadata, e.Metadata))
                            {
                                Metadata = e.Metadata;

                                hasChanged = true;
                            }

                            EnsureProperties();

                            return hasChanged;
                        }

                    case AssetMoved e when e.ParentId != ParentId:
                        {
                            ParentId = e.ParentId;

                            EnsureProperties();

                            return true;
                        }

                    case AssetDeleted:
                        {
                            IsDeleted = true;

                            return true;
                        }
                }

                return false;
            }

            private void EnsureProperties()
            {
                if (Tags == null)
                {
                    Tags = new HashSet<string>();
                }

                if (Metadata == null)
                {
                    Metadata = new AssetMetadata();
                }
            }
        }
    }
}
