// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core;

public sealed class Contributors : ReadonlyDictionary<string, string>
{
    public static readonly Contributors Empty = new Contributors();

    private Contributors()
    {
    }

    public Contributors(IDictionary<string, string> inner)
        : base(inner)
    {
    }

    [Pure]
    public Contributors Assign(string contributorId, string role)
    {
        Guard.NotNullOrEmpty(contributorId);
        Guard.NotNullOrEmpty(role);

        if (!this.TrySet(contributorId, role, out var updated))
        {
            return this;
        }

        return new Contributors(updated);
    }

    [Pure]
    public Contributors Remove(string contributorId)
    {
        Guard.NotNullOrEmpty(contributorId);

        if (!this.TryRemove(contributorId, out var updated))
        {
            return this;
        }

        return new Contributors(updated);
    }
}
