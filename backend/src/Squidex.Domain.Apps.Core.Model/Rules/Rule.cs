﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Flows.Internal;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules;

public record Rule : AppEntity
{
    public string? Name { get; init; }

    public RuleTrigger Trigger { get; init; }

    public FlowDefinition Flow { get; init; }

    public bool IsEnabled { get; init; } = true;

    public override DomainId UniqueId
    {
        get => DomainId.Combine(AppId.Id, Id);
    }

    [Pure]
    public Rule Rename(string? newName)
    {
        if (string.Equals(Name, newName, StringComparison.Ordinal))
        {
            return this;
        }

        return this with { Name = newName };
    }

    [Pure]
    public Rule Enable()
    {
        if (IsEnabled)
        {
            return this;
        }

        return this with { IsEnabled = true };
    }

    [Pure]
    public Rule Disable()
    {
        if (!IsEnabled)
        {
            return this;
        }

        return this with { IsEnabled = false };
    }

    [Pure]
    public Rule Update(RuleTrigger newTrigger)
    {
        Guard.NotNull(newTrigger);

        if (newTrigger.GetType() != Trigger.GetType())
        {
            ThrowHelper.ArgumentException("New trigger has another type.", nameof(newTrigger));
        }

        if (Trigger.Equals(newTrigger))
        {
            return this;
        }

        return this with { Trigger = newTrigger };
    }

    [Pure]
    public Rule Update(FlowDefinition newFlow)
    {
        Guard.NotNull(newFlow);

        if (Flow.Equals(newFlow))
        {
            return this;
        }

        return this with { Flow = newFlow };
    }
}
