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

        public async Task<List<IRuleEntity>> GetRulesAsync(Guid appId)
        {
            var ids = await Index(appId).GetRuleIdsAsync();

            var rules =
                await Task.WhenAll(
                    ids.Select(id => GetRuleAsync(appId, id)));

            return rules.Where(x => x != null).ToList();
        }

        private async Task<IRuleEntity> GetRuleAsync(Guid appId, Guid id)
        {
            var ruleEntity = await grainFactory.GetGrain<IRuleGrain>(id).GetStateAsync();

            if (IsFound(ruleEntity.Value))
            {
                return ruleEntity.Value;
            }

            await Index(appId).RemoveRuleAsync(id);

            return null;
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
            var ruleId = command.RuleId;

            var ruleEntity = await grainFactory.GetGrain<IRuleGrain>(ruleId).GetStateAsync();

            if (IsFound(ruleEntity.Value))
            {
                await Index(ruleEntity.Value.AppId.Id).RemoveRuleAsync(ruleId);
            }
        }

        private IRulesByAppIndex Index(Guid appId)
        {
            return grainFactory.GetGrain<IRulesByAppIndex>(appId);
        }

        private static bool IsFound(IRuleEntity rule)
        {
            return rule.Version > EtagVersion.Empty && !rule.IsDeleted;
        }
    }
}
