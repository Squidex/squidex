// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

public sealed class MoveAssetFolderDto
{
    /// <summary>
    /// The parent folder id.
    /// </summary>
    public DomainId ParentId { get; set; }

    public MoveAssetFolder ToCommand(DomainId id)
    {
        return SimpleMapper.Map(this, new MoveAssetFolder { AssetFolderId = id });
    }
}
