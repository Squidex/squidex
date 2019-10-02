// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class BackupRules : BackupHandler
    {
        private readonly HashSet<Guid> ruleIds = new HashSet<Guid>();
        private readonly IRulesIndex indexForRules;

        public override string Name { get; } = "Rules";

        public BackupRules(IRulesIndex indexForRules)
        {
            Guard.NotNull(indexForRules);

            this.indexForRules = indexForRules;
        }

        public override Task<bool> RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
        {
            switch (@event.Payload)
            {
                case RuleCreated ruleCreated:
                    ruleIds.Add(ruleCreated.RuleId);
                    break;
                case RuleDeleted ruleDeleted:
                    ruleIds.Remove(ruleDeleted.RuleId);
                    break;
            }

            return TaskHelper.True;
        }

        public override Task RestoreAsync(Guid appId, BackupReader reader)
        {
            return indexForRules.RebuildAsync(appId, ruleIds);
        }
    }
}
