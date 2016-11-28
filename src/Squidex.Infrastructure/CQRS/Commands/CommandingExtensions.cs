// ==========================================================================
//  CommandingExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public static class CommandingExtensions
    {
        public static T CreateNew<T>(this IDomainObjectFactory factory, Guid id) where T : IAggregate
        {
            return (T)factory.CreateNew(typeof(T), id);
        }

        public static Task CreateAsync<T>(this IAggregateHandler handler, IAggregateCommand command, Action<T> creator) where T : class, IAggregate
        {
            return handler.CreateAsync<T>(command, x =>
            {
                creator(x);

                return TaskHelper.Done;
            });
        }

        public static Task UpdateAsync<T>(this IAggregateHandler handler, IAggregateCommand command, Action<T> creator) where T : class, IAggregate
        {
            return handler.UpdateAsync<T>(command, x =>
            {
                creator(x);

                return TaskHelper.Done;
            });
        }
    }
}
