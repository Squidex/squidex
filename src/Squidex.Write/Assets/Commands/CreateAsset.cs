// ==========================================================================
//  CreateAsset.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Assets;

namespace Squidex.Write.Assets.Commands
{
    public sealed class CreateAsset : AssetAggregateCommand
    {
        public AssetFile File { get; set; }

        public ImageInfo ImageInfo { get; set; }
    }
}
