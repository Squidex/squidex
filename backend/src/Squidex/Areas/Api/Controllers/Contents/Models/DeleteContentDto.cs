// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Contents.Models
{
    public sealed class DeleteContentDto
    {
        /// <summary>
        /// True to check referrers of this content.
        /// </summary>
        [FromQuery]
        public bool CheckReferrers { get; set; }

        /// <summary>
        /// True to delete the content permanently.
        /// </summary>
        [FromQuery]
        public bool Permanent { get; set; }

        public DeleteContent ToCommand(DomainId id)
        {
            return SimpleMapper.Map(this, new DeleteContent { ContentId = id });
        }
    }
}
