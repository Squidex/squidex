﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public static class CommandExtensions
    {
        public static Task HandleAsync(this ICommandMiddleware commandMiddleware, CommandContext context)
        {
            return commandMiddleware.HandleAsync(context, () => TaskHelper.Done);
        }
    }
}
