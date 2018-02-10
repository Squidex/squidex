// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Commands
{
    public sealed class CreateRule : RuleEditCommand, IAppCommand
    {
        public NamedId<Guid> AppId { get; set; }

        public CreateRule()
        {
            RuleId = Guid.NewGuid();
        }
    }
}
