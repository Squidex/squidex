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
    public interface IAggregateHandler
    {
        Task<T> CreateAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IDomainObject;

        Task<T> CreateSyncedAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IDomainObject;

        Task<T> UpdateAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IDomainObject;

        Task<T> UpdateSyncedAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IDomainObject;
    }
}
