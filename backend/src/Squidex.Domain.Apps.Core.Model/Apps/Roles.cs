// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Core.Apps;

public sealed class Roles
{
    private const int MaxResolved = 1000;
    private readonly ConcurrentDictionary<(string App, string Name, bool IsFrontend), Role?> resolved = new ConcurrentDictionary<(string, string, bool), Role?>();
    private readonly ReadonlyDictionary<string, Role> inner;

    public static readonly IReadOnlyDictionary<string, Role> Defaults = new Dictionary<string, Role>
    {
        [Role.Owner] =
            new Role(Role.Owner,
                new PermissionSet(
                    WithoutPrefix(PermissionIds.App)),
                JsonValue.Object()),
        [Role.Reader] =
            new Role(Role.Reader,
                new PermissionSet(
                    WithoutPrefix(PermissionIds.AppAssetsRead),
                    WithoutPrefix(PermissionIds.AppContentsRead)),
                JsonValue.Object()
                    .Add("ui.api.hide", true)),
        [Role.Editor] =
            new Role(Role.Editor,
                new PermissionSet(
                    WithoutPrefix(PermissionIds.AppAssets),
                    WithoutPrefix(PermissionIds.AppContents),
                    WithoutPrefix(PermissionIds.AppRolesRead),
                    WithoutPrefix(PermissionIds.AppWorkflowsRead)),
                JsonValue.Object()
                    .Add("ui.api.hide", true)),
        [Role.Developer] =
            new Role(Role.Developer,
                new PermissionSet(
                    WithoutPrefix(PermissionIds.AppAssets),
                    WithoutPrefix(PermissionIds.AppContents),
                    WithoutPrefix(PermissionIds.AppRolesRead),
                    WithoutPrefix(PermissionIds.AppRules),
                    WithoutPrefix(PermissionIds.AppSchemas),
                    WithoutPrefix(PermissionIds.AppWorkflows)),
                JsonValue.Object()),
    };

    public static readonly Roles Empty = new Roles(new ReadonlyDictionary<string, Role>());

    public int CustomCount
    {
        get => inner.Count;
    }

    public Role this[string name]
    {
        get => inner[name];
    }

    public IEnumerable<Role> Custom
    {
        get => inner.Values;
    }

    public IEnumerable<Role> All
    {
        get => inner.Values.Union(Defaults.Values);
    }

    private Roles(ReadonlyDictionary<string, Role> roles)
    {
        inner = roles;
    }

    public Roles(Dictionary<string, Role> roles)
    {
        inner = new ReadonlyDictionary<string, Role>(Cleaned(roles));
    }

    [Pure]
    public Roles Remove(string name)
    {
        if (!inner.TryRemove(name, out var updated))
        {
            return this;
        }

        return Create(new ReadonlyDictionary<string, Role>(updated));
    }

    [Pure]
    public Roles Add(string name)
    {
        if (IsDefault(name))
        {
            return this;
        }

        var newRole = new Role(name, null, JsonValue.Object());

        if (!inner.TryAdd(name, newRole, out var updated))
        {
            return this;
        }

        return Create(new ReadonlyDictionary<string, Role>(updated));
    }

    [Pure]
    public Roles Update(string name, PermissionSet? permissions = null, JsonObject? properties = null)
    {
        Guard.NotNullOrEmpty(name);

        if (!inner.TryGetValue(name, out var role))
        {
            return this;
        }

        var newRole = role.Update(permissions, properties);

        if (!inner.TrySet(name, newRole, out var updated))
        {
            return this;
        }

        return Create(new ReadonlyDictionary<string, Role>(updated));
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
        Guard.NotNull(app);

        // Resolving a role builds a permission for every permission of the role, but the result only
        // depends on the key and the roles are immutable, so it is only done once. This is called for
        // every request.
        value = resolved.GetOrAdd((app, name, isFrontend), static (key, self) => self.Resolve(key.App, key.Name, key.IsFrontend), this)!;

        return value != null;
    }

    private Role? Resolve(string app, string name, bool isFrontend)
    {
        // Apps without custom roles share the same empty instance, so this cache is not bound to a
        // single app and could grow with the number of apps. Start over when it gets too large.
        if (resolved.Count >= MaxResolved)
        {
            resolved.Clear();
        }

        if (Defaults.TryGetValue(name, out var role))
        {
            return role.ForApp(app, isFrontend && name != Role.Owner);
        }

        if (inner.TryGetValue(name, out role))
        {
            return role.ForApp(app, isFrontend);
        }

        return null;
    }

    private static string WithoutPrefix(string permission)
    {
        permission = PermissionIds.ForApp(permission).Id;

        var prefix = PermissionIds.ForApp(PermissionIds.App);

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

    private Roles Create(ReadonlyDictionary<string, Role> newRoles)
    {
        return ReferenceEquals(inner, newRoles) ? this : new Roles(newRoles);
    }
}
