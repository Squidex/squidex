﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetAnnotated))]
    public sealed class AssetAnnotated : AssetEvent
    {
        public string FileName { get; set; }

        public string Slug { get; set; }

        public AssetMetadata? Metadata { get; set; }

        public HashSet<string>? Tags { get; set; }
    }
}
