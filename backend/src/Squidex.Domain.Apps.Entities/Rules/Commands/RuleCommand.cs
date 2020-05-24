// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.Commands
{
    public abstract class RuleCommand : AppCommandBase, IAggregateCommand
    {
        public DomainId RuleId { get; set; }

        DomainId IAggregateCommand.AggregateId
        {
            get { return DomainId.Combine(AppId, RuleId); }
        }
    }
}
