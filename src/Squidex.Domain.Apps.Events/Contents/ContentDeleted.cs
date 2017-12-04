﻿// ==========================================================================
//  ContentDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Contents
{
    [EventType(nameof(ContentDeleted))]
    public sealed class ContentDeleted : ContentEvent
    {
    }
}
