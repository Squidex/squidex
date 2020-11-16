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
using Squidex.Infrastructure.Json.Objects;
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
                new Role(Role.Owner,
                    new PermissionSet(
                        Clean(Permissions.App)),
                    JsonValue.Object()),
            [Role.Reader] =
                new Role(Role.Reader,
                    new PermissionSet(
                        Clean(Permissions.AppAssetsRead),
                        Clean(Permissions.AppContentsRead)),
                    JsonValue.Object()
                        .Add("ui.api.hide", true)),
            [Role.Editor] =
                new Role(Role.Editor,
                    new PermissionSet(
                        Clean(Permissions.AppAssets),
                        Clean(Permissions.AppContents),
                        Clean(Permissions.AppRolesRead),
                        Clean(Permissions.AppWorkflowsRead)),
                    JsonValue.Object()
                        .Add("ui.api.hide", true)),
            [Role.Developer] =
                new Role(Role.Developer,
                    new PermissionSet(
                        Clean(Permissions.AppAssets),
                        Clean(Permissions.AppContents),
                        Clean(Permissions.AppPatterns),
                        Clean(Permissions.AppRolesRead),
                        Clean(Permissions.AppRules),
                        Clean(Permissions.AppSchemas),
                        Clean(Permissions.AppWorkflows)),
                    JsonValue.Object())
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
            if (inner.ContainsKey(name))
            {
                return this;
            }

            if (IsDefault(name))
            {
                return this;
            }

            var newRole = Role.Create(name);

            return Create(inner.With(name, newRole));
        }

        [Pure]
        public Roles Update(string name, PermissionSet? permissions = null, JsonObject? properties = null)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            if (!inner.TryGetValue(name, out var role))
            {
                return this;
            }

            var newRole = role.Update(permissions, properties);

            return Create(inner.With(name, newRole));
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

        public bool TryGet(string app, string name, bool isFrontend, [MaybeNullWhen(false)] out Role value)
        {
            Guard.NotNull(app, nameof(app));

            value = null!;

            if (Defaults.TryGetValue(name, out var role))
            {
                value = role.ForApp(app, isFrontend && name != Role.Owner);
            }
            else if (inner.TryGetValue(name, out role))
            {
                value = role.ForApp(app, isFrontend);
            }

            return value != null;
        }

        private static string Clean(string permission)
        {
            permission = Permissions.ForApp(permission).Id;

            var prefix = Permissions.ForApp(Permissions.App);

            if (permission.StartsWith(prefix.Id, StringComparison.OrdinalIgnoreCase))
            {
                permission = permission[prefix.Id.Length..];
            }

            if (permission.Length == 0)
            {
                return Permission.Any;
            }

            return permission[1..];
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
