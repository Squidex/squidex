// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(FieldDisabled))]
    public sealed class FieldDisabled : FieldEvent
    {
    }
}
