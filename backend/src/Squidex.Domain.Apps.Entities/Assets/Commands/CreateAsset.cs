// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class CreateAsset : UploadAssetCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public HashSet<string> Tags { get; set; }

        public CreateAsset()
        {
            AssetId = Guid.NewGuid();
        }
    }
}
