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
        public static void MustHavePermission(this ContentOperation operation, string permissionId)
        {
            var content = operation.Snapshot;

            if (Equals(content.CreatedBy, operation.Actor) || operation.User == null)
            {
                return;
            }

            if (!operation.User.Allows(permissionId, content.AppId.Name, content.SchemaId.Name))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
