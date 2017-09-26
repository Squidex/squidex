// ==========================================================================
//  AssetCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetCreated))]
    public sealed class AssetCreated : AssetEvent
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public long FileVersion { get; set; }

        public long FileSize { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }
    }
}
