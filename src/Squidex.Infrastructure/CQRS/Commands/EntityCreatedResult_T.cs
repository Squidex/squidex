// ==========================================================================
//  EntityCreatedResult_T.cs
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
}
