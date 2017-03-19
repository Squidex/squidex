// ==========================================================================
//  IAggregateHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public interface IAggregateHandler
    {
        Task CreateAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IAggregate;

        Task UpdateAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IAggregate;
    }
}
