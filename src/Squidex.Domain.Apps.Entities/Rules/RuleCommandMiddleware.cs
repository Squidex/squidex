// ==========================================================================
//  RuleCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Guards;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;

        public RuleCommandMiddleware(IAggregateHandler handler, IAppProvider appProvider)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appProvider, nameof(appProvider));

            this.handler = handler;

            this.appProvider = appProvider;
        }

        protected Task On(CreateRule command, CommandContext context)
        {
            return handler.CreateSyncedAsync<RuleDomainObject>(context, async w =>
            {
                await GuardRule.CanCreate(command, appProvider);

                w.Create(command);
            });
        }

        protected Task On(UpdateRule command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<RuleDomainObject>(context, async c =>
            {
                await GuardRule.CanUpdate(command, appProvider);

                c.Update(command);
            });
        }

        protected Task On(EnableRule command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<RuleDomainObject>(context, r =>
            {
                GuardRule.CanEnable(command, r.State.RuleDef);

                r.Enable(command);
            });
        }

        protected Task On(DisableRule command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<RuleDomainObject>(context, r =>
            {
                GuardRule.CanDisable(command, r.State.RuleDef);

                r.Disable(command);
            });
        }

        protected Task On(DeleteRule command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<RuleDomainObject>(context, c =>
            {
                GuardRule.CanDelete(command);

                c.Delete(command);
            });
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (!await this.DispatchActionAsync(context.Command, context))
            {
                await next();
            }
        }
    }
}
