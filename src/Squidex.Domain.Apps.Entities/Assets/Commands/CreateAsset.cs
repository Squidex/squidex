// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class CreateAsset : AssetAggregateCommand
    {
        public AssetFile File { get; set; }

        public ImageInfo ImageInfo { get; set; }

        public CreateAsset()
        {
            AssetId = Guid.NewGuid();
        }
    }
}
