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
    public sealed class AssetCreatedResult : EntitySavedResult
    {
        public Guid Id { get; }

        public HashSet<string> Tags { get; }

        public AssetCreatedResult(Guid id, HashSet<string> tags, long version)
            : base(version)
        {
            Id = id;

            Tags = tags;
        }
    }
}
