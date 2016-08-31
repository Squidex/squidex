// ==========================================================================
//  ICommandHandler.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public interface ICommandHandler
    {
        Task<bool> HandleAsync(CommandContext context);
    }
}
