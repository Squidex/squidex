// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public static class CommandExtensions
    {
        public static Task<T> CreateAsync<T>(this IAggregateHandler handler, CommandContext context, Action<T> creator) where T : class, IDomainObject
        {
            return handler.CreateAsync(context, creator.ToAsync());
        }

        public static Task<T> UpdateAsync<T>(this IAggregateHandler handler, CommandContext context, Action<T> updater) where T : class, IDomainObject
        {
            return handler.UpdateAsync(context, updater.ToAsync());
        }

        public static Task<T> CreateSyncedAsync<T>(this IAggregateHandler handler, CommandContext context, Action<T> creator) where T : class, IDomainObject
        {
            return handler.CreateSyncedAsync(context, creator.ToAsync());
        }

        public static Task<T> UpdateSyncedAsync<T>(this IAggregateHandler handler, CommandContext context, Action<T> updater) where T : class, IDomainObject
        {
            return handler.UpdateSyncedAsync(context, updater.ToAsync());
        }

        public static Task HandleAsync(this ICommandMiddleware commandMiddleware, CommandContext context)
        {
            return commandMiddleware.HandleAsync(context, () => TaskHelper.Done);
        }
    }
}
