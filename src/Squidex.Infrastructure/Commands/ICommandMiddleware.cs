// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public interface ICommandMiddleware
    {
        Task HandleAsync(CommandContext context, Func<Task> next);
    }
}
