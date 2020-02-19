// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentDraftDeleted))]
    public sealed class ContentDraftDeleted : ContentEvent
    {
    }
}
