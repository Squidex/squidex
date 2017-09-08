// ==========================================================================
//  FieldEnabled.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(FieldEnabled))]
    public sealed class FieldEnabled : FieldEvent
    {
    }
}
