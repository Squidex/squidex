// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
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

            if (!context.User.Allows(permissionId, content.AppId.Name, content.SchemaId.Name))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
