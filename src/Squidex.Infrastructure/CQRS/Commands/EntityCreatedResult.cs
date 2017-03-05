// ==========================================================================
//  EntityCreatedResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class EntityCreatedResult<T> : EntitySavedResult
    {
        public T IdOrValue { get; }

        public EntityCreatedResult(T idOrValue, long version)
            : base(version)
        {
            IdOrValue = idOrValue;
        }
    }

    public static class EntityCreatedResult
    {
        public static EntityCreatedResult<T> Create<T>(T idOrValue, long version)
        {
            return new EntityCreatedResult<T>(idOrValue, version);
        }
    }
}
