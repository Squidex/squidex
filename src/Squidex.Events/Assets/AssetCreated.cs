// ==========================================================================
//  AssetCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Events.Assets
{
    [TypeName("AssetCreatedEvent")]
    public class AssetCreated : AssetEvent
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public long FileSize { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }
    }
}
