// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Rules.Commands
{
    public sealed class CreateRule : RuleEditCommand
    {
        public CreateRule()
        {
            RuleId = Guid.NewGuid();
        }
    }
}
