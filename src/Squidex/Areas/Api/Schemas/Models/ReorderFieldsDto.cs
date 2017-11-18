// ==========================================================================
//  ReorderFieldsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Schemas.Models
{
    public sealed class ReorderFieldsDto
    {
        /// <summary>
        /// The field ids in the target order.
        /// </summary>
        [Required]
        public List<long> FieldIds { get; set; }
    }
}
