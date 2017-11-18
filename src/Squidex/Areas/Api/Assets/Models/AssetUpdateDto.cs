// ==========================================================================
//  AssetUpdateDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetUpdateDto
    {
        /// <summary>
        /// The new name of the asset.
        /// </summary>
        [Required]
        public string FileName { get; set; }
    }
}
