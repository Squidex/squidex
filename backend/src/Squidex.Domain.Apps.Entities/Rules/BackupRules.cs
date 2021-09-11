﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class BackupRules : IBackupHandler
    {
        private const int BatchSize = 100;
        private readonly HashSet<DomainId> ruleIds = new HashSet<DomainId>();
        private readonly Rebuilder rebuilder;

        public string Name { get; } = "Rules";

        public BackupRules(Rebuilder rebuilder)
        {
            this.rebuilder = rebuilder;
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

        public async Task RestoreAsync(RestoreContext context)
        {
            if (ruleIds.Count > 0)
            {
                await rebuilder.InsertManyAsync<RuleDomainObject, RuleDomainObject.State>(ruleIds, BatchSize);
            }
        }
    }
}
