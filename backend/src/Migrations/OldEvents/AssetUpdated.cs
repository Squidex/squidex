// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using AssetUpdatedV2 = Squidex.Domain.Apps.Events.Assets.AssetUpdated;

namespace Migrations.OldEvents
{
    [TypeName("AssetUpdated")]
    public sealed class AssetUpdated : AssetEvent, IMigrated<IEvent>
    {
        public string MimeType { get; set; }

        public string FileHash { get; set; }

        public long FileSize { get; set; }

        public long FileVersion { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }

        public IEvent Migrate()
        {
            var result = SimpleMapper.Map(this, new AssetUpdatedV2());

            result.Metadata = new AssetMetadata();

            if (IsImage && PixelWidth != null && PixelHeight != null)
            {
                result.Type = AssetType.Image;

                result.Metadata.SetPixelWidth(PixelWidth.Value);
                result.Metadata.SetPixelHeight(PixelHeight.Value);
            }

            return result;
        }
    }
}
