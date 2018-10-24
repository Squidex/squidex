// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class ApiPermissionUnifier : IAsyncActionFilter
    {
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            var identity = user.Identities.First();

            if (string.Equals(identity.FindFirst(identity.RoleClaimType)?.Value, SquidexRoles.Administrator))
            {
                identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.Admin));
            }

            return next();
        }
    }
}
