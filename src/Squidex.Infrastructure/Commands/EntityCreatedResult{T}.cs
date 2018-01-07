// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands
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
