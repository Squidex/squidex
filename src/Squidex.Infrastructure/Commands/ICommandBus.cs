// ==========================================================================
//  ICommandBus.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface ICommandBus
    {
        Task<CommandContext> PublishAsync(ICommand command);
    }
}
