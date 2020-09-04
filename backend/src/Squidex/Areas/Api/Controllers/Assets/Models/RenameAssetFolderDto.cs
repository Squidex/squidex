﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class RenameAssetFolderDto
    {
        /// <summary>
        /// The name of the folder.
        /// </summary>
        [LocalizedRequired]
        public string FolderName { get; set; }

        public RenameAssetFolder ToCommand(Guid id)
        {
            return SimpleMapper.Map(this, new RenameAssetFolder { AssetFolderId = id });
        }
    }
}
