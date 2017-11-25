// ==========================================================================
//  RuleCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Write.Rules.Commands;
using Squidex.Domain.Apps.Write.Rules.Guards;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Write.Rules
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
            return handler.CreateAsync<RuleDomainObject>(context, async w =>
            {
                await GuardRule.CanCreate(command, appProvider);

                w.Create(command);
            });
        }

        protected Task On(UpdateRule command, CommandContext context)
        {
            return handler.UpdateAsync<RuleDomainObject>(context, async c =>
            {
                await GuardRule.CanUpdate(command, appProvider);

                c.Update(command);
            });
        }

        protected Task On(EnableRule command, CommandContext context)
        {
            return handler.UpdateAsync<RuleDomainObject>(context, r =>
            {
                GuardRule.CanEnable(command, r.Rule);

                r.Enable(command);
            });
        }

        protected Task On(DisableRule command, CommandContext context)
        {
            return handler.UpdateAsync<RuleDomainObject>(context, r =>
            {
                GuardRule.CanDisable(command, r.Rule);

                r.Disable(command);
            });
        }

        protected Task On(DeleteRule command, CommandContext context)
        {
            return handler.UpdateAsync<RuleDomainObject>(context, c =>
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
