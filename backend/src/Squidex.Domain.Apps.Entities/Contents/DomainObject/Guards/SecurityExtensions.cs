// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class SecurityExtensions
    {
        public static void MustHavePermission(this OperationContext context, string permissionId)
        {
            var content = context.Content;

            if (Equals(content.CreatedBy, context.Actor) || context.User == null)
            {
                return;
            }

            var permissions = context.User?.Claims.Permissions();

            if (permissions?.Allows(new Permission(permissionId)) == true)
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
