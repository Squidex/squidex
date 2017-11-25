// ==========================================================================
//  UpdateFieldDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class UpdateFieldDto
    {
        /// <summary>
        /// The field properties.
        /// </summary>
        [Required]
        public FieldPropertiesDto Properties { get; set; }
    }
}
