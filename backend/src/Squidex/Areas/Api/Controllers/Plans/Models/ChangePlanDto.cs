// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Plans.Models
{
    public sealed class ChangePlanDto
    {
        /// <summary>
        /// The new plan id.
        /// </summary>
        [LocalizedRequired]
        public string PlanId { get; set; }

        public ChangePlan ToCommand(HttpContext httpContext)
        {
            var result = SimpleMapper.Map(this, new ChangePlan());

            result.Referer = httpContext.Request.Headers["Referer"];

            return result;
        }
    }
}
