// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.Commands
{
    public abstract class RuleCommand : SquidexCommand, IAppCommand, IAggregateCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public DomainId RuleId { get; set; }

        public DomainId AggregateId
        {
            get => DomainId.Combine(AppId, RuleId);
        }
    }
}
