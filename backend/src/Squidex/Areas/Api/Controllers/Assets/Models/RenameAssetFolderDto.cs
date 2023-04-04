// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

[OpenApiRequest]
public sealed class RenameAssetFolderDto
{
    /// <summary>
    /// The name of the folder.
    /// </summary>
    [LocalizedRequired]
    public string FolderName { get; set; }

    public RenameAssetFolder ToCommand(DomainId id)
    {
        return SimpleMapper.Map(this, new RenameAssetFolder { AssetFolderId = id });
    }
}
