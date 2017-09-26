// ==========================================================================
//  AssetSavedResult.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Domain.Apps.Write.Assets
{
    public class AssetSavedResult : EntitySavedResult
    {
        public long FileVersion { get; }

        public AssetSavedResult(long version, long fileVersion)
            : base(version)
        {
            FileVersion = fileVersion;
        }
    }
}
