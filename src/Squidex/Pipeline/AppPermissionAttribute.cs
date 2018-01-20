// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public abstract class AppPermissionAttribute : ActionFilterAttribute
    {
        private readonly AppPermission requestedPermission;

        protected AppPermissionAttribute(AppPermission requestedPermission)
        {
            this.requestedPermission = requestedPermission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var app = context.HttpContext.Features.Get<IAppFeature>()?.App;

            if (app != null)
            {
                var user = context.HttpContext.User;

                var permission =
                    FindByOpenIdSubject(app, user) ??
                    FindByOpenIdClient(app, user);

                if (permission == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                if (permission.Value > requestedPermission)
                {
                    context.Result = new StatusCodeResult(403);
                    return;
                }

                var defaultIdentity = context.HttpContext.User.Identities.First();

                var additionalRoles = new List<string>
                {
                    SquidexRoles.AppReader
                };

                if (permission.Value <= AppPermission.Editor)
                {
                    additionalRoles.Add(SquidexRoles.AppEditor);
                }

                if (permission.Value <= AppPermission.Developer)
                {
                    additionalRoles.Add(SquidexRoles.AppDeveloper);
                }

                if (permission.Value <= AppPermission.Owner)
                {
                    additionalRoles.Add(SquidexRoles.AppOwner);
                }

                foreach (var role in additionalRoles)
                {
                    defaultIdentity.AddClaim(new Claim(defaultIdentity.RoleClaimType, role));
                }
            }
        }

        private static AppPermission? FindByOpenIdClient(IAppEntity app, ClaimsPrincipal user)
        {
            var clientId = user.GetClientId();

            if (clientId != null && app.Clients.TryGetValue(clientId, out var client))
            {
                return client.Permission.ToAppPermission();
            }

            return null;
        }

        private static AppPermission? FindByOpenIdSubject(IAppEntity app, ClaimsPrincipal user)
        {
            var subjectId = user.FindFirst(OpenIdClaims.Subject)?.Value;

            if (subjectId != null && app.Contributors.TryGetValue(subjectId, out var permission))
            {
                return permission.ToAppPermission();
            }

            return null;
        }
    }
}
