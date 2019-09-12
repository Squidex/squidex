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
                        ids.Select(id => GetRuleAsync(appId, id)));

                return rules.Where(x => x != null).ToList();
            }
        }

        private async Task<IRuleEntity> GetRuleAsync(Guid appId, Guid id)
        {
            using (Profiler.TraceMethod<RulesIndex>())
            {
                var ruleEntity = await grainFactory.GetGrain<IRuleGrain>(id).GetStateAsync();

                if (IsFound(ruleEntity.Value))
                {
                    return ruleEntity.Value;
                }

                await Index(appId).RemoveRuleAsync(id);

                return null;
            }
        }

        private async Task<List<Guid>> GetRuleIdsAsync(Guid appId)
        {
            using (Profiler.TraceMethod<RulesIndex>())
            {
                return await Index(appId).GetRuleIdsAsync();
            }
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CreateRule createRule)
            {
                await CreateRuleAsync(createRule);
            }

            await next();

            if (context.IsCompleted)
            {
                if (context.Command is DeleteRule deleteRule)
                {
                    await DeleteRuleAsync(deleteRule);
                }
            }
        }

        private async Task CreateRuleAsync(CreateRule command)
        {
            await Index(command.AppId.Id).AddRuleAsync(command.RuleId);
        }

        private async Task DeleteRuleAsync(DeleteRule command)
        {
            var id = command.RuleId;

            var rule = await grainFactory.GetGrain<IRuleGrain>(id).GetStateAsync();

            if (IsFound(rule.Value))
            {
                await Index(rule.Value.AppId.Id).RemoveRuleAsync(id);
            }
        }

        private IRulesByAppIndexGrain Index(Guid appId)
        {
            return grainFactory.GetGrain<IRulesByAppIndexGrain>(appId);
        }

        private static bool IsFound(IRuleEntity rule)
        {
            return rule.Version > EtagVersion.Empty && !rule.IsDeleted;
        }
    }
}
