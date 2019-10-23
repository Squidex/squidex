// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class Roles
    {
        private readonly ArrayDictionary<string, Role> inner;

        public static readonly IReadOnlyDictionary<string, Role> Defaults = new Dictionary<string, Role>
        {
            [Role.Owner] =
                new Role(Role.Owner, new PermissionSet(
                    Clean(Permissions.App))),
            [Role.Reader] =
                new Role(Role.Reader, new PermissionSet(
                    Clean(Permissions.AppAssetsRead),
                    Clean(Permissions.AppContentsRead))),
            [Role.Editor] =
                new Role(Role.Editor, new PermissionSet(
                    Clean(Permissions.AppAssets),
                    Clean(Permissions.AppContents),
                    Clean(Permissions.AppRolesRead),
                    Clean(Permissions.AppWorkflowsRead))),
            [Role.Developer] =
                new Role(Role.Developer, new PermissionSet(
                    Clean(Permissions.AppApi),
                    Clean(Permissions.AppAssets),
                    Clean(Permissions.AppContents),
                    Clean(Permissions.AppPatterns),
                    Clean(Permissions.AppRolesRead),
                    Clean(Permissions.AppRules),
                    Clean(Permissions.AppSchemas),
                    Clean(Permissions.AppWorkflows)))
        };

        public static readonly Roles Empty = new Roles();

        public int CustomCount
        {
            get { return inner.Count; }
        }

        public Role this[string name]
        {
            get { return inner[name]; }
        }

        public IEnumerable<Role> Custom
        {
            get { return inner.Values; }
        }

        public IEnumerable<Role> All
        {
            get { return inner.Values.Union(Defaults.Values); }
        }

        private Roles(ArrayDictionary<string, Role> roles = null)
        {
            inner = roles ?? new ArrayDictionary<string, Role>();
        }

        public Roles(IEnumerable<KeyValuePair<string, Role>> items)
        {
            inner = new ArrayDictionary<string, Role>(items.Where(x => !Defaults.ContainsKey(x.Key)).ToArray());
        }

        [Pure]
        public Roles Remove(string name)
        {
            return new Roles(inner.Without(name));
        }

        [Pure]
        public Roles Add(string name)
        {
            var newRole = new Role(name);

            if (inner.ContainsKey(name))
            {
                throw new ArgumentException("Name already exists.", nameof(name));
            }

            return new Roles(inner.With(name, newRole));
        }

        [Pure]
        public Roles Update(string name, params string[] permissions)
        {
            Guard.NotNullOrEmpty(name, nameof(name));
            Guard.NotNull(permissions, nameof(permissions));

            if (!inner.TryGetValue(name, out var role))
            {
                return this;
            }

            return new Roles(inner.With(name, role.Update(permissions)));
        }

        public static bool IsDefault(string role)
        {
            return role != null && Defaults.ContainsKey(role);
        }

        public static bool IsDefault(Role role)
        {
            return role != null && Defaults.ContainsKey(role.Name);
        }

        public bool IsCustom(string name)
        {
            return inner.ContainsKey(name);
        }

        public bool IsAny(string name)
        {
            return inner.ContainsKey(name) || Defaults.ContainsKey(name);
        }

        public bool TryGetCustom(string name, out Role role)
        {
            return inner.TryGetValue(name, out role);
        }

        public bool TryGet(string app, string name, out Role value)
        {
            Guard.NotNull(app, nameof(app));

            value = null;

            if (Defaults.TryGetValue(name, out var role) || inner.TryGetValue(name, out role))
            {
                value = role.ForApp(app);
                return true;
            }

            return false;
        }

        private static string Clean(string permission)
        {
            permission = Permissions.ForApp(permission, "*", "*").Id;

            var prefix = Permissions.ForApp(Permissions.App);

            if (permission.StartsWith(prefix.Id, StringComparison.OrdinalIgnoreCase))
            {
                permission = permission.Substring(prefix.Id.Length);
            }

            if (permission.Length == 0)
            {
                return "*";
            }

            return permission.Substring(1);
        }
    }
}
