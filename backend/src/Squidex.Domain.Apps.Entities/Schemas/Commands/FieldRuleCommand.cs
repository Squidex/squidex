// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class FieldRuleCommand
    {
        public FieldRuleAction Action { get; set; }

        public string Field { get; set; }

        public string? Condition { get; set; }

        public FieldRule ToFieldRule()
        {
            return new FieldRule(Action, Field)
            {
                Condition = Condition
            };
        }
    }
}
