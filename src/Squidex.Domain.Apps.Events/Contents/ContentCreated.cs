// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentCreated), 2)]
    public sealed class ContentCreated : ContentEvent
    {
        public Status Status { get; set; }

        public NamedContentData Data { get; set; }
    }
}
