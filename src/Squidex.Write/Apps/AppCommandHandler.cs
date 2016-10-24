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
using Squidex.Write.Apps.Commands;

namespace Squidex.Write.Apps
{
    public class AppCommandHandler : CommandHandler<AppDomainObject>
    {
        private readonly IAppRepository appRepository;

        public AppCommandHandler(
            IDomainObjectFactory domainObjectFactory, 
            IDomainObjectRepository domainObjectRepository,
            IAppRepository appRepository) 
            : base(domainObjectFactory, domainObjectRepository)
        {
            Guard.NotNull(appRepository, nameof(appRepository));

            this.appRepository = appRepository;
        }

        public async Task On(CreateApp command)
        {
            if (await appRepository.FindAppByNameAsync(command.Name) != null)
            {
                var error = new ValidationError($"A app with name '{command.Name}' already exists", "Name");

                throw new ValidationException("Cannot create a new app", error);
            }

            await CreateAsync(command, x => x.Create(command));
        }

        public override Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command);
        }
    }
}
