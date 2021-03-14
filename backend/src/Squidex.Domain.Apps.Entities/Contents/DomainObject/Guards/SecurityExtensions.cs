// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class SecurityExtensions
    {
        public static void MustHavePermission(this OperationContext context, params string[] permissions)
        {
            var content = context.Content;

            if (Equals(content.CreatedBy, context.Actor) || context.User == null)
            {
                return;
            }

            if (permissions.All(x => !context.User.Allows(x, content.AppId.Name, content.SchemaId.Name)))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
