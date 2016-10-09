// ==========================================================================
//  CommandingExtensions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;

namespace PinkParrot.Infrastructure.CQRS.Commands
{
    public static class CommandingExtensions
    {
        public static T CreateNew<T>(this IDomainObjectFactory factory, Guid id) where T : IAggregate
        {
            return (T)factory.CreateNew(typeof(T), id);
        }
    }
}
