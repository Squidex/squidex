// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public delegate Task NextDelegate(CommandContext context);

    public interface ICommandMiddleware
    {
        Task HandleAsync(CommandContext context, NextDelegate next);
    }
}
