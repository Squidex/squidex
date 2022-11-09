// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands;

public sealed class CreateAssetFolder : AssetFolderCommand
{
    public string FolderName { get; set; }

    public DomainId ParentId { get; set; }

    public bool OptimizeValidation { get; set; }

    public CreateAssetFolder()
    {
        AssetFolderId = DomainId.NewGuid();
    }
}
