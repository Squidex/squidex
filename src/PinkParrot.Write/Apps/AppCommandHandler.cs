// ==========================================================================
//  AppCommandHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using PinkParrot.Infrastructure.CQRS.Commands;
using PinkParrot.Infrastructure.Dispatching;
using PinkParrot.Write.Apps.Commands;

namespace PinkParrot.Write.Apps
{
    public class AppCommandHandler : CommandHandler<AppDomainObject>
    {
        public AppCommandHandler(
            IDomainObjectFactory domainObjectFactory, 
            IDomainObjectRepository domainObjectRepository) 
            : base(domainObjectFactory, domainObjectRepository)
        {
        }

        public Task On(CreateApp command)
        {
            return CreateAsync(command, x => x.Create(command));
        }

        public override Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command);
        }
    }
}
