// ==========================================================================
//  EntityCreatedResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.CQRS.Commands
{
    public static class EntityCreatedResult
    {
        public static EntityCreatedResult<T> Create<T>(T idOrValue, long version)
        {
            return new EntityCreatedResult<T>(idOrValue, version);
        }
    }
}
