// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class FieldRule
    {
        public FieldRuleAction Action { get; }

        public string Field { get; }

        public string? Condition { get; }

        public FieldRule(FieldRuleAction action, string field, string? condition)
        {
            Guard.Enum(action, nameof(action));
            Guard.NotNullOrEmpty(field, nameof(field));

            Action = action;

            Field = field;

            Condition = condition;
        }

        public static FieldRule Disable(string field, string? condition = null)
        {
            return new FieldRule(FieldRuleAction.Disable, field, condition);
        }

        public static FieldRule Hide(string field, string? condition = null)
        {
            return new FieldRule(FieldRuleAction.Hide, field, condition);
        }
    }
}
