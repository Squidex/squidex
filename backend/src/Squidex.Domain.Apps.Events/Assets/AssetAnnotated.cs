﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetAnnotated))]
    public sealed class AssetAnnotated : AssetItemEvent
    {
        public string FileName { get; set; }

        public string Slug { get; set; }

        public HashSet<string>? Tags { get; set; }
    }
}
