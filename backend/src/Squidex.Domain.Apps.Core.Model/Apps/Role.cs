// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.Apps;

public sealed record Role(string Name, PermissionSet? Permissions = null, JsonObject? Properties = null)
{
    private static readonly HashSet<string> ExtraPermissions = new HashSet<string>
    {
        PermissionIds.AppComments,
        PermissionIds.AppContributorsRead,
        PermissionIds.AppHistory,
        PermissionIds.AppLanguagesRead,
        PermissionIds.AppPing,
        PermissionIds.AppRolesRead,
        PermissionIds.AppSchemasRead,
        PermissionIds.AppSearch,
        PermissionIds.AppTranslate,
        PermissionIds.AppUsage
    };

    public const string Editor = "Editor";
    public const string Developer = "Developer";
    public const string Owner = "Owner";
    public const string Reader = "Reader";

    public string Name { get; } = Guard.NotNullOrEmpty(Name);

    public JsonObject Properties { get; } = Properties ?? new JsonObject();

    public PermissionSet Permissions { get; } = Permissions ?? PermissionSet.Empty;

    public bool IsDefault
    {
        get => Roles.IsDefault(this);
    }

    public static Role WithPermissions(string name, params string[] permissions)
    {
        return new Role(name, new PermissionSet(permissions), new JsonObject());
    }

    public static Role WithProperties(string name, JsonObject properties)
    {
        return new Role(name, PermissionSet.Empty, properties);
    }

    [Pure]
    public Role Update(PermissionSet? permissions, JsonObject? properties)
    {
        return new Role(Name, permissions ?? Permissions, properties ?? Properties);
    }

    public bool Equals(string name)
    {
        return name != null && name.Equals(Name, StringComparison.Ordinal);
    }

    public Role ForApp(string app, bool isFrontend = false)
    {
        Guard.NotNullOrEmpty(app);

        var result = new HashSet<Permission>();

        if (Permissions.Any())
        {
            var prefix = PermissionIds.ForApp(PermissionIds.App, app).Id;

            foreach (var permission in Permissions)
            {
                result.Add(new Permission(string.Concat(prefix, ".", permission.Id)));
            }
        }

        if (isFrontend)
        {
            foreach (var extraPermissionId in ExtraPermissions)
            {
                var extraPermission = PermissionIds.ForApp(extraPermissionId, app);

                result.Add(extraPermission);
            }
        }

        return new Role(Name, new PermissionSet(result), Properties);
    }
}
