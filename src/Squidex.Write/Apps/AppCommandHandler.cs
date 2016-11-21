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
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Users.Repositories;
using Squidex.Write.Apps.Commands;

namespace Squidex.Write.Apps
{
    public class AppCommandHandler : CommandHandler<AppDomainObject>
    {
        private readonly IAppRepository appRepository;
        private readonly IUserRepository userRepository;

        public AppCommandHandler(
            IDomainObjectFactory domainObjectFactory, 
            IDomainObjectRepository domainObjectRepository,
            IAppRepository appRepository, 
            IUserRepository userRepository) 
            : base(domainObjectFactory, domainObjectRepository)
        {
            Guard.NotNull(appRepository, nameof(appRepository));
            Guard.NotNull(userRepository, nameof(userRepository));

            this.appRepository = appRepository;
            this.userRepository = userRepository;
        }

        public Task On(CreateApp command)
        {
            return CreateAsync(command, async x =>
            {
                if (await appRepository.FindAppByNameAsync(command.Name) != null)
                {
                    var error = new ValidationError($"A app with name '{command.Name}' already exists", nameof(CreateApp.Name));

                    throw new ValidationException("Cannot create a new app", error);
                }

                x.Create(command);
            });
        }

        public Task On(AssignContributor command)
        {
            return UpdateAsync(command, async x =>
            {
                if (await userRepository.FindUserByIdAsync(command.ContributorId) == null)
                {
                    var error = new ValidationError($"Cannot find contributor '{command.ContributorId ?? "UNKNOWN"}'", nameof(AssignContributor.ContributorId));

                    throw new ValidationException("Cannot assign contributor to app", error);
                }

                x.AssignContributor(command);
            });
        }

        public Task On(RemoveContributor command)
        {
            return UpdateAsync(command, x => x.RemoveContributor(command));
        }

        public Task On(CreateClientKey command)
        {
            return UpdateAsync(command, x => x.CreateClientKey(command));
        }

        public Task On(RevokeClientKey command)
        {
            return UpdateAsync(command, x => x.RevokeClientKey(command));
        }

        public Task On(ConfigureLanguages command)
        {
            return UpdateAsync(command, x => x.ConfigureLanguages(command));
        }

        public override Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command);
        }
    }
}
