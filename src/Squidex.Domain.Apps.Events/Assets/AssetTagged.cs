// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetTagged))]
    public sealed class AssetTagged : AssetEvent
    {
        public HashSet<string> Tags { get; set; }
    }
}
