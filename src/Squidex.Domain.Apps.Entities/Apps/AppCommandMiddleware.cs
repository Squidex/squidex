// ==========================================================================
//  AppCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Guards;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppPlanBillingManager appPlansBillingManager;
        private readonly IUserResolver userResolver;

        public AppCommandMiddleware(
            IAggregateHandler handler,
            IAppProvider appProvider,
            IAppPlansProvider appPlansProvider,
            IAppPlanBillingManager appPlansBillingManager,
            IUserResolver userResolver)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(userResolver, nameof(userResolver));
            Guard.NotNull(appPlansProvider, nameof(appPlansProvider));
            Guard.NotNull(appPlansBillingManager, nameof(appPlansBillingManager));

            this.handler = handler;
            this.userResolver = userResolver;
            this.appProvider = appProvider;
            this.appPlansProvider = appPlansProvider;
            this.appPlansBillingManager = appPlansBillingManager;
        }

        protected Task On(CreateApp command, CommandContext context)
        {
            return handler.CreateSyncedAsync<AppDomainObject>(context, async a =>
            {
                await GuardApp.CanCreate(command, appProvider);

                a.Create(command);

                context.Complete(EntityCreatedResult.Create(a.State.Id, a.Version));
            });
        }

        protected Task On(AssignContributor command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, async a =>
            {
                await GuardAppContributors.CanAssign(a.State.Contributors, command, userResolver, appPlansProvider.GetPlan(a.State.Plan?.PlanId));

                a.AssignContributor(command);
            });
        }

        protected Task On(RemoveContributor command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppContributors.CanRemove(a.State.Contributors, command);

                a.RemoveContributor(command);
            });
        }

        protected Task On(AttachClient command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppClients.CanAttach(a.State.Clients, command);

                a.AttachClient(command);
            });
        }

        protected Task On(UpdateClient command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppClients.CanUpdate(a.State.Clients, command);

                a.UpdateClient(command);
            });
        }

        protected Task On(RevokeClient command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppClients.CanRevoke(a.State.Clients, command);

                a.RevokeClient(command);
            });
        }

        protected Task On(AddLanguage command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppLanguages.CanAdd(a.State.LanguagesConfig, command);

                a.AddLanguage(command);
            });
        }

        protected Task On(RemoveLanguage command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppLanguages.CanRemove(a.State.LanguagesConfig, command);

                a.RemoveLanguage(command);
            });
        }

        protected Task On(UpdateLanguage command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppLanguages.CanUpdate(a.State.LanguagesConfig, command);

                a.UpdateLanguage(command);
            });
        }

        protected Task On(ChangePlan command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, async a =>
            {
                GuardApp.CanChangePlan(command, a.State.Plan, appPlansProvider);

                if (command.FromCallback)
                {
                    a.ChangePlan(command);
                }
                else
                {
                    var result = await appPlansBillingManager.ChangePlanAsync(command.Actor.Identifier, a.State.Id, a.State.Name, command.PlanId);

                    if (result is PlanChangedResult)
                    {
                        a.ChangePlan(command);
                    }

                    context.Complete(result);
                }
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
