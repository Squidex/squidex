// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Assets;

public record AssetFolder : AssetItem
{
    public string FolderName { get; init; }

    [Pure]
    public AssetFolder Move(DomainId parentId)
    {
        if (Equals(ParentId, parentId))
        {
            return this;
        }

        return this with { ParentId = parentId };
    }

    [Pure]
    public AssetFolder Rename(string folderName)
    {
        Guard.NotNull(folderName);

        if (string.Equals(FolderName, folderName, StringComparison.Ordinal))
        {
            return this;
        }

        return this with { FolderName = folderName };
    }
}
