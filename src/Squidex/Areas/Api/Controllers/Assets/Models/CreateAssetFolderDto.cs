// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class CreateAssetFolderDto
    {
        /// <summary>
        /// The folder name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        public CreateAssetFolder ToCommand(Guid folderId)
        {
            return SimpleMapper.Map(this, new CreateAssetFolder { FolderId = folderId });
        }
    }
}
