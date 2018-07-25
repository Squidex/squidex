// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup.Handlers
{
    public sealed class RestoreRules : HandlerBase, IRestoreHandler
    {
        private readonly HashSet<Guid> ruleIds = new HashSet<Guid>();
        private readonly IGrainFactory grainFactory;
        private Guid appId;

        public string Name { get; } = "Rules";

        public RestoreRules(IStore<Guid> store, IGrainFactory grainFactory)
            : base(store)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task HandleAsync(Envelope<IEvent> @event, Stream attachment)
        {
            switch (@event.Payload)
            {
                case AppCreated appCreated:
                    appId = appCreated.AppId.Id;
                    break;
                case RuleCreated ruleCreated:
                    ruleIds.Add(ruleCreated.RuleId);
                    break;
                case RuleDeleted ruleDeleted:
                    ruleIds.Remove(ruleDeleted.RuleId);
                    break;
            }

            return TaskHelper.Done;
        }

        public async Task ProcessAsync()
        {
            await RebuildManyAsync(ruleIds, id => RebuildAsync<RuleState, RuleGrain>(id, (e, s) => s.Apply(e)));

            await grainFactory.GetGrain<IRulesByAppIndex>(appId).RebuildAsync(ruleIds);
        }

        public Task CompleteAsync()
        {
            return TaskHelper.Done;
        }
    }
}
