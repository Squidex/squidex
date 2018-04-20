// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetUpdateDto
    {
        /// <summary>
        /// The new name of the asset.
        /// </summary>
        [Required]
        public string FileName { get; set; }

        public RenameAsset ToCommand(Guid id)
        {
            return SimpleMapper.Map(this, new RenameAsset { AssetId = id });
        }
    }
}
