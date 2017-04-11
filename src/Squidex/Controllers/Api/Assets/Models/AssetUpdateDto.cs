// ==========================================================================
//  AssetUpdateDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Assets.Models
{
    public class AssetUpdateDto
    {
        /// <summary>
        /// The new name of the asset.
        /// </summary>
        [Required]
        public string Name { get; set; }
    }
}
