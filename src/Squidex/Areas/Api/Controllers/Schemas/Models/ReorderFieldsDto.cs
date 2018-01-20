// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
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
