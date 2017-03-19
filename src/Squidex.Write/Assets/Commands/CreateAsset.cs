// ==========================================================================
//  CreateAsset.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Write.Assets.Commands
{
    public sealed class CreateAsset : AssetAggregateCommand
    {
        public string FileName { get; set; }

        public string MimeType { get; set; }

        public long FileSize { get; set; }

        public bool IsImage { get; set; }
    }
}
