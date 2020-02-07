// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
        public NamedId<Guid> AppId { get; set; }

        public Guid ParentId { get; set; }

        public string FileName { get; set; }

        public string FileHash { get; set; }

        public string MimeType { get; set; }

        public string Slug { get; set; }

        public long FileVersion { get; set; }

        public long FileSize { get; set; }

        public long TotalSize { get; set; }

        public bool IsProtected { get; set; }

        public HashSet<string> Tags { get; set; }

        public AssetMetadata Metadata { get; set; }

        public AssetType Type { get; set; }

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

                        EnsureProperties();

                        return true;
                    }

                case AssetUpdated e:
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

                        if (Is.OptionalChange(Tags, e.Tags))
                        {
                            Tags = e.Tags;

                            hasChanged = true;
                        }

                        if (Is.OptionalChange(Metadata, e.Metadata))
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
