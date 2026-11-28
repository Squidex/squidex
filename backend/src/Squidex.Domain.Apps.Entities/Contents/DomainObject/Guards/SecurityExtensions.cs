// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards;

public static class SecurityExtensions
{
    public static void MustHavePermission(this ContentOperation context, string permissionId)
    {
        var content = context.Snapshot;

        if (Equals(content.CreatedBy, context.Actor))
        {
            return;
        }

        context.MustHaveUserPermission(permissionId);
    }

    public static void MustHaveUserPermission(this ContentOperation context, string permissionId)
    {
        if (context.User == null)
        {
            return;
        }

        var permissions = context.User.Claims.Permissions();

        if (permissions == null)
        {
            throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
        }

        if (!permissions.Allows(permissionId, context.App.Name, context.Schema.Name))
        {
            throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
        }
    }

    public static void MustNotGrantMorePermissions(this ContentOperation context, ContentData data)
    {
        if (context.User == null)
        {
            return;
        }

        // Every top level value with an API key and a role is indexed and can be used to authenticate.
        var userInfos = data.ToUserInfos();
        if (userInfos == null)
        {
            return;
        }

        var userInfosOld = new HashSet<UserInfoValue>();

        // API keys that already exist have been created by someone else and can be kept when other values change.
        userInfosOld.UnionWith(context.Snapshot.CurrentVersion?.Data?.ToUserInfos() ?? []);
        userInfosOld.UnionWith(context.Snapshot.NewVersion?.Data?.ToUserInfos() ?? []);

        var permissions = context.User.Claims.Permissions() ?? PermissionSet.Empty;

        foreach (var userInfo in userInfos)
        {
            if (userInfosOld.Contains(userInfo))
            {
                continue;
            }

            // Unknown roles cannot be used to authenticate.
            if (!context.App.TryGetRole(userInfo.Role, false, out var role))
            {
                continue;
            }

            if (!role.Permissions.All(permissions.Allows))
            {
                throw new DomainForbiddenException(T.Get("common.errorNoPermission"));
            }
        }
    }
}
