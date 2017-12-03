// ==========================================================================
//  FieldShown.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(FieldShown))]
    public sealed class FieldShown : FieldEvent
    {
    }
}
