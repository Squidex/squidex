// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class BackupRules : BackupHandlerWithStore
    {
        private readonly HashSet<Guid> ruleIds = new HashSet<Guid>();
        private readonly IGrainFactory grainFactory;
        private readonly IRuleEventRepository ruleEventRepository;

        public override string Name { get; } = "Rules";

        public BackupRules(IStore<Guid> store, IGrainFactory grainFactory, IRuleEventRepository ruleEventRepository)
            : base(store)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));

            this.grainFactory = grainFactory;

            this.ruleEventRepository = ruleEventRepository;
        }

        public override Task RestoreEventAsync(Envelope<IEvent> @event, Guid appId, BackupReader reader, RefToken actor)
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

            return TaskHelper.Done;
        }

        public async override Task RestoreAsync(Guid appId, BackupReader reader)
        {
            await RebuildManyAsync(ruleIds, id => RebuildAsync<RuleState, RuleGrain>(id, (e, s) => s.Apply(e)));

            await grainFactory.GetGrain<IRulesByAppIndex>(appId).RebuildAsync(ruleIds);
        }
    }
}
