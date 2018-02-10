// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class UpdateAsset : AssetCommand
    {
        public AssetFile File { get; set; }

        public ImageInfo ImageInfo { get; set; }
    }
}
