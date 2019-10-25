// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands
{
    public class EntitySavedResult
    {
        public long Version { get; }

        public EntitySavedResult(long version)
        {
            Version = version;
        }
    }
}
