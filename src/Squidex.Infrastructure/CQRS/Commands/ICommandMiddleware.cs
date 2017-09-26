// ==========================================================================
//  ICommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface ICommandMiddleware
    {
        Task HandleAsync(CommandContext context, Func<Task> next);
    }
}
