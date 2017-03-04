// ==========================================================================
//  AppCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Users.Repositories;
using Squidex.Write.Apps.Commands;

namespace Squidex.Write.Apps
{
    public class AppCommandHandler : ICommandHandler
    {
        private readonly IAggregateHandler handler;
        private readonly IAppRepository appRepository;
        private readonly IUserRepository userRepository;
        private readonly ClientKeyGenerator keyGenerator;

        public AppCommandHandler(
            IAggregateHandler handler,
            IAppRepository appRepository,
            IUserRepository userRepository,
            ClientKeyGenerator keyGenerator)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(keyGenerator, nameof(keyGenerator));
            Guard.NotNull(appRepository, nameof(appRepository));
            Guard.NotNull(userRepository, nameof(userRepository));

            this.handler = handler;
            this.keyGenerator = keyGenerator;
            this.appRepository = appRepository;
            this.userRepository = userRepository;
        }

        protected async Task On(CreateApp command, CommandContext context)
        {
            if (await appRepository.FindAppAsync(command.Name) != null)
            {
                var error =
                    new ValidationError($"An app with name '{command.Name}' already exists",
                        nameof(CreateApp.Name));

                throw new ValidationException("Cannot create a new app", error);
            }

            await handler.CreateAsync<AppDomainObject>(context, x => x.Create(command));
        }

        protected async Task On(AssignContributor command, CommandContext context)
        {
            if (await userRepository.FindUserByIdAsync(command.ContributorId) == null)
            {
                var error =
                    new ValidationError("Cannot find contributor the contributor",
                        nameof(AssignContributor.ContributorId));

                throw new ValidationException("Cannot assign contributor to app", error);
            }

            await handler.UpdateAsync<AppDomainObject>(context, x => x.AssignContributor(command));
        }

        protected Task On(AttachClient command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, x =>
            {
                x.AttachClient(command, keyGenerator.GenerateKey());

                context.Succeed(new EntityCreatedResult<AppClient>(x.Clients[command.Id], x.Version));
            });
        }

        protected Task On(RemoveContributor command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, x => x.RemoveContributor(command));
        }

        protected Task On(RenameClient command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, x => x.RenameClient(command));
        }

        protected Task On(RevokeClient command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, x => x.RevokeClient(command));
        }

        protected Task On(AddLanguage command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, x => x.AddLanguage(command));
        }

        protected Task On(RemoveLanguage command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, x => x.RemoveLanguage(command));
        }

        protected Task On(SetMasterLanguage command, CommandContext context)
        {
            return handler.UpdateAsync<AppDomainObject>(context, x => x.SetMasterLanguage(command));
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? TaskHelper.False : this.DispatchActionAsync(context.Command, context);
        }
    }
}
