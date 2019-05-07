// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetCreatedResult : EntityCreatedResult<Guid>
    {
        public HashSet<string> Tags { get; }

        public long FileVersion { get; }

        public string FileHash { get; }

        public bool IsDuplicate { get; }

        public AssetCreatedResult(Guid id, HashSet<string> tags, long version, long fileVersion, string fileHash, bool isDuplicate)
            : base(id, version)
        {
            Tags = tags;

            FileVersion = fileVersion;
            FileHash = fileHash;

            IsDuplicate = isDuplicate;
        }
    }
}
