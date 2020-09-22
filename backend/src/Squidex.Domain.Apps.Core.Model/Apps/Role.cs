// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;
using P = Squidex.Shared.Permissions;

namespace Squidex.Domain.Apps.Core.Apps
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class Role : Named
    {
        private static readonly HashSet<string> ExtraPermissions = new HashSet<string>
        {
            P.AppComments,
            P.AppContributorsRead,
            P.AppHistory,
            P.AppHistory,
            P.AppLanguagesRead,
            P.AppPatternsRead,
            P.AppPing,
            P.AppRolesRead,
            P.AppSchemasRead,
            P.AppSearch,
            P.AppTranslate,
            P.AppUsage
        };

        public const string Editor = "Editor";
        public const string Developer = "Developer";
        public const string Owner = "Owner";
        public const string Reader = "Reader";

        public static readonly ReadOnlyCollection<string> EmptyProperties = new ReadOnlyCollection<string>(new List<string>());

        public PermissionSet Permissions { get; }

        public JsonObject Properties { get; }

        [IgnoreDuringEquals]
        public bool IsDefault
        {
            get { return Roles.IsDefault(this); }
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
                var prefix = P.ForApp(P.App, app).Id;

                foreach (var permission in Permissions)
                {
                    result.Add(new Permission(string.Concat(prefix, ".", permission.Id)));
                }
            }

            if (isFrontend)
            {
                foreach (var extraPermissionId in ExtraPermissions)
                {
                    var extraPermission = P.ForApp(extraPermissionId, app);

                    result.Add(extraPermission);
                }
            }

            return new Role(Name, new PermissionSet(result), Properties);
        }
    }
}
