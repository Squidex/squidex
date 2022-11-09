// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Core.ValidateContent;

public sealed record ValidationContext(RootContext Root)
{
    public ImmutableQueue<string> Path { get; init; } = ImmutableQueue<string>.Empty;

    public bool IsOptional { get; init; }

    public ValidationMode Mode { get; init; }

    public ValidationAction Action { get; init; }

    public void AddError(IEnumerable<string> path, string message)
    {
        Root.AddError(path, message);
    }

    public ValidationContext Optimized(bool optimized = true)
    {
        return WithMode(optimized ? ValidationMode.Optimized : ValidationMode.Default);
    }

    public ValidationContext AsPublishing(bool publish = true)
    {
        return WithAction(publish ? ValidationAction.Publish : ValidationAction.Upsert);
    }

    public ValidationContext Optional(bool isOptional = true)
    {
        if (IsOptional == isOptional)
        {
            return this;
        }

        return this with { IsOptional = isOptional };
    }

    public ValidationContext WithAction(ValidationAction action)
    {
        if (Action == action)
        {
            return this;
        }

        return this with { Action = action };
    }

    public ValidationContext WithMode(ValidationMode mode)
    {
        if (Mode == mode)
        {
            return this;
        }

        return this with { Mode = mode };
    }

    public ValidationContext Nested(string property)
    {
        return this with { Path = Path.Enqueue(property) };
    }

    public ValidationContext Nested(string property, bool isOptional)
    {
        return this with { Path = Path.Enqueue(property), IsOptional = isOptional };
    }
}
