// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class ContentsIdsQueryDto
    {
        /// <summary>
        /// The list of ids to query.
        /// </summary>
        [LocalizedRequired]
        public List<DomainId> Ids { get; set; }
    }
}
