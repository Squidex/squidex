// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public sealed class RulesByAppIndexCommandMiddleware : ICommandMiddleware
    {
        private readonly IGrainFactory grainFactory;

        public RulesByAppIndexCommandMiddleware(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.IsCompleted)
            {
                switch (context.Command)
                {
                    case CreateRule createRule:
                        await Index(createRule.AppId.Id).AddRuleAsync(createRule.RuleId);
                        break;
                    case DeleteRule deleteRule:
                        {
                            var schema = await grainFactory.GetGrain<IRuleGrain>(deleteRule.RuleId).GetStateAsync();

                            await Index(schema.Value.AppId.Id).RemoveRuleAsync(deleteRule.RuleId);

                            break;
                        }
                }
            }

            await next();
        }

        private IRulesByAppIndex Index(Guid appId)
        {
            return grainFactory.GetGrain<IRulesByAppIndex>(appId);
        }
    }
}
