// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetMoved))]
    public sealed class AssetMoved : AssetEvent
    {
        public Guid ParentId { get; set; }
    }
}
