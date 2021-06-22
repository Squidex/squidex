// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesIndex : ICommandMiddleware, IRulesIndex
    {
        private readonly IGrainFactory grainFactory;

        public RulesIndex(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        public Task RebuildAsync(DomainId appId, HashSet<DomainId> rules)
        {
            return Index(appId).RebuildAsync(rules);
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(DomainId appId)
        {
            using (Profiler.TraceMethod<RulesIndex>())
            {
                var ids = await GetRuleIdsAsync(appId);

                var rules =
                    await Task.WhenAll(
                        ids.Select(id => GetRuleCoreAsync(DomainId.Combine(appId, id))));

                return rules.NotNull().ToList();
            }
        }

        private async Task<List<DomainId>> GetRuleIdsAsync(DomainId appId)
        {
            using (Profiler.TraceMethod<RulesIndex>())
            {
                return await Index(appId).GetIdsAsync();
            }
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await next(context);

            if (context.IsCompleted)
            {
                switch (context.Command)
                {
                    case CreateRule createRule:
                        await CreateRuleAsync(createRule);
                        break;
                    case DeleteRule deleteRule:
                        await DeleteRuleAsync(deleteRule);
                        break;
                }
            }
        }

        private async Task CreateRuleAsync(CreateRule command)
        {
            await Index(command.AppId.Id).AddAsync(command.RuleId);
        }

        private async Task DeleteRuleAsync(DeleteRule command)
        {
            var rule = await GetRuleCoreAsync(command.AggregateId);

            if (rule != null)
            {
                await Index(rule.AppId.Id).RemoveAsync(rule.Id);
            }
        }

        private IRulesByAppIndexGrain Index(DomainId appId)
        {
            return grainFactory.GetGrain<IRulesByAppIndexGrain>(appId.ToString());
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
