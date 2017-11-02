// ==========================================================================
//  CreateRule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Write.Rules.Commands
{
    public sealed class CreateRule : RuleEditCommand
    {
        public CreateRule()
        {
            RuleId = Guid.NewGuid();
        }
    }
}
