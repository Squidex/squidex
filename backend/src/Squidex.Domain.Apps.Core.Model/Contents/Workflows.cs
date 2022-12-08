// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Contents;

public sealed class Workflows : ReadonlyDictionary<DomainId, Workflow>
{
    public static readonly Workflows Empty = new Workflows();

    private Workflows()
    {
    }

    public Workflows(IDictionary<DomainId, Workflow> inner)
        : base(inner)
    {
    }

    [Pure]
    public Workflows Remove(DomainId id)
    {
        if (!this.TryRemove(id, out var updated))
        {
            return this;
        }

        return new Workflows(updated);
    }

    [Pure]
    public Workflows Add(DomainId workflowId, string name)
    {
        Guard.NotNullOrEmpty(name);

        if (!this.TryAdd(workflowId, Workflow.CreateDefault(name), out var updated))
        {
            return this;
        }

        return new Workflows(updated);
    }

    [Pure]
    public Workflows Set(Workflow workflow)
    {
        Guard.NotNull(workflow);

        if (!this.TrySet(default, workflow, out var updated))
        {
            return this;
        }

        return new Workflows(updated);
    }

    [Pure]
    public Workflows Set(DomainId id, Workflow workflow)
    {
        Guard.NotNull(workflow);

        if (!this.TrySet(id, workflow, out var updated))
        {
            return this;
        }

        return new Workflows(updated);
    }

    [Pure]
    public Workflows Update(DomainId id, Workflow workflow)
    {
        Guard.NotNull(workflow);

        if (id == DomainId.Empty)
        {
            return Set(workflow);
        }

        if (!this.TryUpdate(id, workflow, out var updated))
        {
            return this;
        }

        return new Workflows(updated);
    }

    public Workflow GetFirst()
    {
        return Values.FirstOrDefault() ?? Workflow.Default;
    }
}
