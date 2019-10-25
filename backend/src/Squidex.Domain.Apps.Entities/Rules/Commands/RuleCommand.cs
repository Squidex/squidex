// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.Commands
{
    public abstract class RuleCommand : SquidexCommand, IAggregateCommand
    {
        public Guid RuleId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return RuleId; }
        }
    }
}
