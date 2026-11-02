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

namespace Squidex.Domain.Apps.Entities.Assets.DomainObject.Guards;

public static class SecurityExtensions
{
    public static void MustHavePermission(this AssetOperation operation, string permissionId)
    {
        // Commands without a user are created by the system itself and are not restricted.
        if (operation.User == null)
        {
            return;
        }

        var permissions = operation.User.Claims.Permissions();

        if (!permissions.Allows(permissionId, operation.App.Name))
        {
            throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
        }
    }
}
