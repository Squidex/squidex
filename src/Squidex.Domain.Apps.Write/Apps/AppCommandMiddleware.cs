﻿// ==========================================================================
//  AppCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAppRepository appRepository;
        private readonly IAppPlansProvider appPlansProvider;
        private readonly IAppPlanBillingManager appPlansBillingManager;
        private readonly IUserResolver userResolver;

        public AppCommandMiddleware(
            IAggregateHandler handler,
            IAppRepository appRepository,
            IAppPlansProvider appPlansProvider,
            IAppPlanBillingManager appPlansBillingManager,
            IUserResolver userResolver)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appRepository, nameof(appRepository));
            Guard.NotNull(userResolver, nameof(userResolver));
            Guard.NotNull(appPlansProvider, nameof(appPlansProvider));
            Guard.NotNull(appPlansBillingManager, nameof(appPlansBillingManager));

            this.handler = handler;
            this.userResolver = userResolver;
            this.appRepository = appRepository;
            this.appPlansProvider = appPlansProvider;
            this.appPlansBillingManager = appPlansBillingManager;
        }

        protected async Task On(CreateApp command, CommandContext context)
        {
            if (await appRepository.FindAppAsync(command.Name) != null)
            {
                var error =
                    new ValidationError($"An app with name '{command.Name}' already exists",
                        nameof(CreateApp.Name));

                throw new ValidationException("Cannot create a new app.", error);
            }

            await handler.CreateAsync<AppDomainObject>(context, a =>
            {
                a.Create(command);

                context.Complete(EntityCreatedResult.Create(a.Id, a.Version));
            });
        }

        protected async Task On(AssignContributor command, CommandContext context)
        {
            if (await userResolver.FindByIdAsync(command.ContributorId) == null)
            {
                var error =
                    new ValidationError("Cannot find contributor the contributor.",
                        nameof(AssignContributor.ContributorId));

                throw new ValidationException("Cannot assign contributor to app.", error);
            }

            await handler.UpdateAsync<AppDomainObject>(context, a =>
            {
                var oldContributors = a.ContributorCount;
                var maxContributors = appPlansProvider.GetPlan(a.PlanId).MaxContributors;

                a.AssignContributor(command);

                if (maxContributors > 0 && a.ContributorCount > oldContributors && a.ContributorCount > maxContributors)
                {
                    var error = new ValidationError("You have reached your max number of contributors.");

                    throw new ValidationException("Cannot assign contributor to app.", error);
                }
            });
        }

        protected Task On(ChangePlan command, CommandContext context)
        {
            if (!appPlansProvider.IsConfiguredPlan(command.PlanId))
            {
                var error =
                    new ValidationError($"The plan '{command.PlanId}' does not exists",
                        nameof(CreateApp.Name));

                throw new ValidationException("Cannot change plan.", error);
            }

            return handler.UpdateAsync<AppDomainObject>(context, async a =>
            {
                if (command.FromCallback)
                {
                    a.ChangePlan(command);
                }
                else
                {
                    var result = await appPlansBillingManager.ChangePlanAsync(command.Actor.Identifier, a.Id, a.Name, command.PlanId);

                    if (result is PlanChangedResult)
                    {
                        a.ChangePlan(command);
                    }

                    context.Complete(result);
                }
            });
        }

        protected Task On(AttachClient command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, a => a.AttachClient(command));
        }

        protected Task On(RemoveContributor command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, a => a.RemoveContributor(command));
        }

        protected Task On(UpdateClient command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, a => a.UpdateClient(command));
        }

        protected Task On(RevokeClient command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, a => a.RevokeClient(command));
        }

        protected Task On(AddLanguage command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, a => a.AddLanguage(command));
        }

        protected Task On(RemoveLanguage command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, a => a.RemoveLanguage(command));
        }

        protected Task On(UpdateLanguage command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, a => a.UpdateLanguage(command));
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
