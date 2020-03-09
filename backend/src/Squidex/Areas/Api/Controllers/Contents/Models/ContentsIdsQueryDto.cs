// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentsIdsQueryDto
    {
        /// <summary>
        /// The list of ids to query.
        /// </summary>
        [Required]
        public List<Guid> Ids { get; set; }
    }
}
