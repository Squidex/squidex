// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets
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
