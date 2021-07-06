// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class BackupRules : IBackupHandler
    {
        private readonly HashSet<DomainId> ruleIds = new HashSet<DomainId>();
        private readonly IRulesIndex indexForRules;

        public string Name { get; } = "Rules";

        public BackupRules(IRulesIndex indexForRules)
        {
            this.indexForRules = indexForRules;
        }

        public Task<bool> RestoreEventAsync(Envelope<IEvent> @event, RestoreContext context)
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

            return Task.FromResult(true);
        }

        public Task RestoreAsync(RestoreContext context)
        {
            return indexForRules.RebuildAsync(context.AppId, ruleIds);
        }
    }
}
