// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesIndex : ICommandMiddleware, IRulesIndex
    {
        private readonly IGrainFactory grainFactory;

        public RulesIndex(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(DomainId appId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("RulesIndex/GetRulesAsync"))
            {
                var ids = await GetRuleIdsAsync(appId);

                var rules =
                    await Task.WhenAll(
                        ids.Select(id => GetRuleCoreAsync(DomainId.Combine(appId, id))));

                return rules.NotNull().ToList();
            }
        }

        private async Task<IReadOnlyCollection<DomainId>> GetRuleIdsAsync(DomainId appId)
        {
            using (Telemetry.Activities.StartActivity("RulesIndex/GetRuleIdsAsync"))
            {
                return await Cache(appId).GetRuleIdsAsync();
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await next(context);

            if (context.IsCompleted)
            {
                switch (context.Command)
                {
                    case CreateRule create:
                        await OnCreateAsync(create);
                        break;
                    case DeleteRule delete:
                        await OnDeleteAsync(delete);
                        break;
                }
            }
        }

        private async Task OnCreateAsync(CreateRule create)
        {
            await Cache(create.AppId.Id).AddAsync(create.RuleId);
        }

        private async Task OnDeleteAsync(DeleteRule delete)
        {
            await Cache(delete.AppId.Id).RemoveAsync(delete.RuleId);
        }

        private IRulesCacheGrain Cache(DomainId appId)
        {
            return grainFactory.GetGrain<IRulesCacheGrain>(appId.ToString());
        }

        private async Task<IRuleEntity?> GetRuleCoreAsync(DomainId id)
        {
            var rule = (await grainFactory.GetGrain<IRuleGrain>(id.ToString()).GetStateAsync()).Value;

            if (rule.Version <= EtagVersion.Empty || rule.IsDeleted)
            {
                return null;
            }

            return rule;
        }
    }
}
