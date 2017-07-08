// ==========================================================================
//  EntitySavedResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
namespace Squidex.Infrastructure.CQRS.Commands
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
