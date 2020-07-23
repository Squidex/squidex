// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesIndex : ICommandMiddleware, IRulesIndex
    {
        private readonly IGrainFactory grainFactory;

        public RulesIndex(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task RebuildAsync(Guid appId, HashSet<Guid> rues)
        {
            return Index(appId).RebuildAsync(rues);
        }

        public async Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            using (Profiler.TraceMethod<RulesIndex>())
            {
                var ids = await GetRuleIdsAsync(appId);

                var rules =
                    await Task.WhenAll(
                        ids.Select(GetRuleCoreAsync));

                return rules.NotNull().ToList();
            }
        }

        private async Task<IRuleEntity?> GetRuleAsync(Guid id)
        {
            using (Profiler.TraceMethod<RulesIndex>())
            {
                return await GetRuleCoreAsync(id);
            }
        }

        private async Task<List<Guid>> GetRuleIdsAsync(Guid appId)
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
            var rule = await GetRuleAsync(command.RuleId);

            if (rule != null)
            {
                await Index(rule.AppId.Id).RemoveAsync(rule.Id);
            }
        }

        private IRulesByAppIndexGrain Index(Guid appId)
        {
            return grainFactory.GetGrain<IRulesByAppIndexGrain>(appId);
        }

        private async Task<IRuleEntity?> GetRuleCoreAsync(Guid ruleId)
        {
            var rule = (await grainFactory.GetGrain<IRuleGrain>(ruleId).GetStateAsync()).Value;

            if (rule.Version <= EtagVersion.Empty)
            {
                return null;
            }

            return rule;
        }
    }
}
