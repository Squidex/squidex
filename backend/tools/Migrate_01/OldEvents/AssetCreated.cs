﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using AssetCreatedV2 = Squidex.Domain.Apps.Events.Assets.AssetCreated;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(AssetCreated))]
    [Obsolete]
    public sealed class AssetCreated : AssetEvent, IMigrated<IEvent>
    {
        public Guid ParentId { get; set; }

        public string FileName { get; set; }

        public string FileHash { get; set; }

        public string MimeType { get; set; }

        public string Slug { get; set; }

        public long FileVersion { get; set; }

        public long FileSize { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }

        public HashSet<string>? Tags { get; set; }

        public IEvent Migrate()
        {
            var result = SimpleMapper.Map(this, new AssetCreatedV2());

            result.Metadata = new AssetMetadata();

            if (IsImage && PixelWidth.HasValue && PixelHeight.HasValue)
            {
                result.Type = AssetType.Image;

                result.Metadata.SetPixelWidth(PixelWidth.Value);
                result.Metadata.SetPixelHeight(PixelHeight.Value);
            }

            return result;
        }
    }
}
