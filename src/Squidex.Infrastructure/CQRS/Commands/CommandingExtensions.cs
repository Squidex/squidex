// ==========================================================================
//  CommandingExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public static class CommandingExtensions
    {
        public static T CreateNew<T>(this IDomainObjectFactory factory, Guid id) where T : IAggregate
        {
            return (T)factory.CreateNew(typeof(T), id);
        }
    }
}
