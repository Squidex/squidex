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

                context.Complete(EntityCreatedResult.Create(command.AppId, a.Version));
            });
        }

        protected Task On(AssignContributor command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, async a =>
            {
                await GuardAppContributors.CanAssign(a.Snapshot.Contributors, command, userResolver, appPlansProvider.GetPlan(a.Snapshot.Plan?.PlanId));

                a.AssignContributor(command);
            });
        }

        protected Task On(RemoveContributor command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppContributors.CanRemove(a.Snapshot.Contributors, command);

                a.RemoveContributor(command);
            });
        }

        protected Task On(AttachClient command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppClients.CanAttach(a.Snapshot.Clients, command);

                a.AttachClient(command);
            });
        }

        protected Task On(UpdateClient command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppClients.CanUpdate(a.Snapshot.Clients, command);

                a.UpdateClient(command);
            });
        }

        protected Task On(RevokeClient command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppClients.CanRevoke(a.Snapshot.Clients, command);

                a.RevokeClient(command);
            });
        }

        protected Task On(AddLanguage command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppLanguages.CanAdd(a.Snapshot.LanguagesConfig, command);

                a.AddLanguage(command);
            });
        }

        protected Task On(RemoveLanguage command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppLanguages.CanRemove(a.Snapshot.LanguagesConfig, command);

                a.RemoveLanguage(command);
            });
        }

        protected Task On(UpdateLanguage command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppLanguages.CanUpdate(a.Snapshot.LanguagesConfig, command);

                a.UpdateLanguage(command);
            });
        }

        protected Task On(AddPattern command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppPattern.CanAdd(a.Snapshot.Patterns, command);

                a.AddPattern(command);
            });
        }

        protected Task On(DeletePattern command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppPattern.CanDelete(a.Snapshot.Patterns, command);

                a.DeletePattern(command);
            });
        }

        protected async Task On(UpdatePattern command, CommandContext context)
        {
            await handler.UpdateSyncedAsync<AppDomainObject>(context, a =>
            {
                GuardAppPattern.CanUpdate(a.Snapshot.Patterns, command);

                a.UpdatePattern(command);
            });
        }

        protected Task On(ChangePlan command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AppDomainObject>(context, async a =>
            {
                GuardApp.CanChangePlan(command, a.Snapshot.Plan, appPlansProvider);

                if (command.FromCallback)
                {
                    a.ChangePlan(command);
                }
                else
                {
                    var result = await appPlansBillingManager.ChangePlanAsync(command.Actor.Identifier, command.AppId.Id, a.Snapshot.Name, command.PlanId);

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
