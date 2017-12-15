// ==========================================================================
//  UpdateAsset.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class UpdateAsset : AssetAggregateCommand
    {
        public AssetFile File { get; set; }

        public ImageInfo ImageInfo { get; set; }
    }
}
