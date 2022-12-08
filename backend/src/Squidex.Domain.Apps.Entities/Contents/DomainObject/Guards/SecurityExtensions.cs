// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards;

public static class SecurityExtensions
{
    public static void MustHavePermission(this ContentOperation context, string permissionId)
    {
        var content = context.Snapshot;

        if (Equals(content.CreatedBy, context.Actor) || context.User == null)
        {
            return;
        }

        var permissions = context.User?.Claims.Permissions();

        if (permissions == null)
        {
            throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
        }

        var permission = PermissionIds.ForApp(permissionId, context.App.Name, context.Schema.SchemaDef.Name);

        if (!permissions.Allows(permission))
        {
            throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
        }
    }
}
