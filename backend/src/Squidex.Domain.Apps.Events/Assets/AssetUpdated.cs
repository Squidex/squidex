// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Events.Assets
{
    [TypeName("AssetUpdated")]
    public sealed class AssetUpdated : AssetItemEvent
    {
        public string MimeType { get; set; }

        public string FileHash { get; set; }

        public long FileSize { get; set; }

        public long FileVersion { get; set; }

        public bool IsImage { get; set; }

        public int? PixelWidth { get; set; }

        public int? PixelHeight { get; set; }
    }
}
