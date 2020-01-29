// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection.Equality;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Apps
{
    public sealed class Roles
    {
        private readonly ImmutableDictionary<string, Role> inner;

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

        public static readonly Roles Empty = new Roles(new ImmutableDictionary<string, Role>());

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

        private Roles(ImmutableDictionary<string, Role> roles)
        {
            inner = roles;
        }

        public Roles(Dictionary<string, Role> roles)
        {
            inner = new ImmutableDictionary<string, Role>(Cleaned(roles));
        }

        [Pure]
        public Roles Remove(string name)
        {
            return Create(inner.Without(name));
        }

        [Pure]
        public Roles Add(string name)
        {
            var newRole = new Role(name);

            if (inner.ContainsKey(name))
            {
                throw new ArgumentException("Name already exists.", nameof(name));
            }

            if (IsDefault(name))
            {
                return this;
            }

            return Create(inner.With(name, newRole));
        }

        [Pure]
        public Roles Update(string name, params string[] permissions)
        {
            Guard.NotNullOrEmpty(name);
            Guard.NotNull(permissions);

            if (!inner.TryGetValue(name, out var role))
            {
                return this;
            }

            return Create(inner.With(name, role.Update(permissions), DeepEqualityComparer<Role>.Default));
        }

        public static bool IsDefault(string role)
        {
            return role != null && Defaults.ContainsKey(role);
        }

        public static bool IsDefault(Role role)
        {
            return role != null && Defaults.ContainsKey(role.Name);
        }

        public bool ContainsCustom(string name)
        {
            return inner.ContainsKey(name);
        }

        public bool Contains(string name)
        {
            return inner.ContainsKey(name) || Defaults.ContainsKey(name);
        }

        public bool TryGet(string app, string name, [MaybeNullWhen(false)] out Role value)
        {
            Guard.NotNull(app);

            if (Defaults.TryGetValue(name, out var role) || inner.TryGetValue(name, out role))
            {
                value = role.ForApp(app);
                return true;
            }

            value = null!;

            return false;
        }

        private static string Clean(string permission)
        {
            permission = Permissions.ForApp(permission).Id;

            var prefix = Permissions.ForApp(Permissions.App);

            if (permission.StartsWith(prefix.Id, StringComparison.OrdinalIgnoreCase))
            {
                permission = permission.Substring(prefix.Id.Length);
            }

            if (permission.Length == 0)
            {
                return Permission.Any;
            }

            return permission.Substring(1);
        }

        private static Dictionary<string, Role> Cleaned(Dictionary<string, Role> inner)
        {
            return inner.Where(x => !Defaults.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        }

        private Roles Create(ImmutableDictionary<string, Role> newRoles)
        {
            return ReferenceEquals(inner, newRoles) ? this : new Roles(newRoles);
        }
    }
}
