// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed record Role : Named
    {
        private static readonly HashSet<string> ExtraPermissions = new HashSet<string>
        {
            Shared.Permissions.AppComments,
            Shared.Permissions.AppContributorsRead,
            Shared.Permissions.AppHistory,
            Shared.Permissions.AppLanguagesRead,
            Shared.Permissions.AppPing,
            Shared.Permissions.AppRolesRead,
            Shared.Permissions.AppSchemasRead,
            Shared.Permissions.AppSearch,
            Shared.Permissions.AppTranslate,
            Shared.Permissions.AppUsage
        };

        public const string Editor = "Editor";
        public const string Developer = "Developer";
        public const string Owner = "Owner";
        public const string Reader = "Reader";

        public PermissionSet Permissions { get; }

        public JsonObject Properties { get; }

        public bool IsDefault
        {
            get => Roles.IsDefault(this);
        }

        public Role(string name, PermissionSet permissions, JsonObject properties)
            : base(name)
        {
            Guard.NotNull(permissions, nameof(permissions));
            Guard.NotNull(properties, nameof(properties));

            Permissions = permissions;
            Properties = properties;
        }

        public static Role WithPermissions(string role, params string[] permissions)
        {
            return new Role(role, new PermissionSet(permissions), JsonValue.Object());
        }

        public static Role WithProperties(string role, JsonObject properties)
        {
            return new Role(role, PermissionSet.Empty, properties);
        }

        public static Role Create(string role)
        {
            return new Role(role, PermissionSet.Empty, JsonValue.Object());
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
            Guard.NotNullOrEmpty(app, nameof(app));

            var result = new HashSet<Permission>();

            if (Permissions.Any())
            {
                var prefix = Shared.Permissions.ForApp(Shared.Permissions.App, app).Id;

                foreach (var permission in Permissions)
                {
                    result.Add(new Permission(string.Concat(prefix, ".", permission.Id)));
                }
            }

            if (isFrontend)
            {
                foreach (var extraPermissionId in ExtraPermissions)
                {
                    var extraPermission = Shared.Permissions.ForApp(extraPermissionId, app);

                    result.Add(extraPermission);
                }
            }

            return new Role(Name, new PermissionSet(result), Properties);
        }
    }
}
