// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class CreateAssetFolder : AssetFolderCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public string FolderName { get; set; }

        public Guid ParentId { get; set; }

        public CreateAssetFolder()
        {
            AssetFolderId = Guid.NewGuid();
        }
    }
}
