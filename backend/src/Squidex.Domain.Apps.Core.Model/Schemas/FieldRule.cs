// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed record FieldRule
{
    public FieldRuleAction Action { get; }

    public string Field { get; }

    public string? Condition { get; init; }

    public FieldRule(FieldRuleAction action, string field)
    {
        Guard.Enum(action);
        Guard.NotNullOrEmpty(field);

        Action = action;

        Field = field;
    }

    public static FieldRule Disable(string field, string? condition = null)
    {
        return new FieldRule(FieldRuleAction.Disable, field)
        {
            Condition = condition
        };
    }

    public static FieldRule Hide(string field, string? condition = null)
    {
        return new FieldRule(FieldRuleAction.Hide, field)
        {
            Condition = condition
        };
    }
}
