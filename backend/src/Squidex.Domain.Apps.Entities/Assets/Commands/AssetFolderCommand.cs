// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

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
