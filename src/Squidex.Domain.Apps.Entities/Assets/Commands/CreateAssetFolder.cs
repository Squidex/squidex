// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class CreateAssetFolder : AssetCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public Guid FolderId { get; set; }

        public string Name { get; set; }

        public CreateAssetFolder()
        {
            AssetId = Guid.NewGuid();
        }
    }
}
