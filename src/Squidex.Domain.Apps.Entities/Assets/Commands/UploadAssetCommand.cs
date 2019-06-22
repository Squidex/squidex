// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public abstract class UploadAssetCommand : AssetCommand
    {
        public AssetFile File { get; set; }

        public ImageInfo ImageInfo { get; set; }

        public string FileHash { get; set; }
    }
}
