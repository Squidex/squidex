// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public abstract class AssetFolderCommand : AppCommandBase, IAggregateCommand
    {
        public DomainId AssetFolderId { get; set; }

        DomainId IAggregateCommand.AggregateId
        {
            get { return DomainId.Combine(AppId, AssetFolderId); }
        }
    }
}
