// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Assets.Commands;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetUpdateDto
    {
        /// <summary>
        /// The new name of the asset.
        /// </summary>
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// The new asset tags.
        /// </summary>
        [Required]
        public HashSet<string> Tags { get; set; }

        public AssetCommand ToCommand(Guid id)
        {
            if (Tags != null)
            {
                return new TagAsset { AssetId = id, Tags = Tags };
            }
            else
            {
                return new RenameAsset { AssetId = id, FileName = FileName };
            }
        }
    }
}
