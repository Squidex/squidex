// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.Security;

public sealed class PermissionSet : ReadonlyList<Permission>
{
    public static readonly PermissionSet Empty = new PermissionSet(Array.Empty<string>());

    private readonly Lazy<string> display;

    public PermissionSet(params Permission[] permissions)
        : this(permissions?.ToList()!)
    {
    }

    public PermissionSet(params string[] permissions)
        : this(permissions?.Select(x => new Permission(x)).ToList()!)
    {
    }

    public PermissionSet(IEnumerable<string> permissions)
        : this(permissions?.Select(x => new Permission(x)).ToList()!)
    {
    }

    public PermissionSet(IEnumerable<Permission> permissions)
        : this(permissions?.ToList()!)
    {
    }

    public PermissionSet(IList<Permission> permissions)
        : base(permissions)
    {
        display = new Lazy<string>(() => string.Join(";", this));
    }

    public PermissionSet Add(string permission)
    {
        Guard.NotNullOrEmpty(permission);

        return Add(new Permission(permission));
    }

    public PermissionSet Add(Permission permission)
    {
        Guard.NotNull(permission);

        return new PermissionSet(this.Union(Enumerable.Repeat(permission, 1)).Distinct());
    }

    public bool Allows(Permission? other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Any(x => x.Allows(other));
    }

    public bool Includes(Permission? other)
    {
        if (other == null)
        {
            return false;
        }

        return this.Any(x => x.Includes(other));
    }

    public override string ToString()
    {
        return display.Value;
    }

    public IEnumerable<string> ToIds()
    {
        return this.Select(x => x.Id);
    }
}
