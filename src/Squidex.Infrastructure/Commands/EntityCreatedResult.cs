// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands
{
    public static class EntityCreatedResult
    {
        public static EntityCreatedResult<T> Create<T>(T idOrValue, long version)
        {
            return new EntityCreatedResult<T>(idOrValue, version);
        }
    }
}
