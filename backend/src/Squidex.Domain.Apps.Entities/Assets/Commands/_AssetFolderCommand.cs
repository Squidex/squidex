// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Assets.Commands;

public abstract class AssetFolderCommand : AssetFolderCommandBase
{
    public DomainId AssetFolderId { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, AssetFolderId);
    }
}

// This command is needed as marker for middlewares.
public abstract class AssetFolderCommandBase : SquidexCommand, IAppCommand, IAggregateCommand
{
    public NamedId<DomainId> AppId { get; set; }

    public abstract DomainId AggregateId { get; }
}
