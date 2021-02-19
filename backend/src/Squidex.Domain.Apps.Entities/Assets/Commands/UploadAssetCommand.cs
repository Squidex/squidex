// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public abstract class UploadAssetCommand : AssetCommand
    {
        public HashSet<string> Tags { get; set; } = new HashSet<string>();

        public AssetFile File { get; set; }

        public AssetMetadata Metadata { get; } = new AssetMetadata();

        public AssetType Type { get; set; }

        public string FileHash { get; set; }
    }
}
