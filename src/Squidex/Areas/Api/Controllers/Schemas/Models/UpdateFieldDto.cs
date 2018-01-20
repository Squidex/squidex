// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
