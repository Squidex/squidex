// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class CreateAsset : AssetCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public AssetFile File { get; set; }

        public ImageInfo ImageInfo { get; set; }

        public CreateAsset()
        {
            AssetId = Guid.NewGuid();
        }
    }
}
