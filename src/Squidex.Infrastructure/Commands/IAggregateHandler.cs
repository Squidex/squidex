// ==========================================================================
//  IAggregateHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public interface IAggregateHandler
    {
        Task<T> CreateAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IAggregate;

        Task<T> UpdateAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IAggregate;
    }
}
