// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;

namespace Squidex.Domain.Apps.Entities.Rules.Commands
{
    public abstract class RuleEditCommand : RuleCommand
    {
        public RuleTrigger Trigger { get; set; }

        public RuleAction Action { get; set; }
    }
}
