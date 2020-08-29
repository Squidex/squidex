// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Plans.Models
{
    public sealed class ChangePlanDto
    {
        /// <summary>
        /// The new plan id.
        /// </summary>
        [Required]
        public string PlanId { get; set; }

        public ChangePlan ToCommand(HttpContext httpContext)
        {
            var result = SimpleMapper.Map(this, new ChangePlan());

            result.Referer = httpContext.Request.Headers["Referer"];

            return result;
        }
    }
}
