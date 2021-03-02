// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.EnrichedEvents
{
    public sealed class EnrichedAssetEvent : EnrichedUserEventBase, IEnrichedEntityEvent
    {
        public EnrichedAssetEventType Type { get; set; }

        public DomainId Id { get; set; }

        public Instant Created { get; set; }

        public Instant LastModified { get; set; }

        public RefToken CreatedBy { get; set; }

        public RefToken LastModifiedBy { get; set; }

        public string MimeType { get; set; }

        public string FileName { get; set; }

        public long FileVersion { get; set; }

        public long FileSize { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }

        public AssetType AssetType { get; set; }

        public bool IsImage
        {
            get => AssetType == AssetType.Image;
        }

        public override long Partition
        {
            get => Id.GetHashCode();
        }
    }
}
