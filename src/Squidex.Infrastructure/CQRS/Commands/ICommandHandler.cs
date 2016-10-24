// ==========================================================================
//  ICommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface ICommandHandler
    {
        Task<bool> HandleAsync(CommandContext context);
    }
}
