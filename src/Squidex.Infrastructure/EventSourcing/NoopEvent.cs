// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    [TypeName(nameof(NoopEvent))]
    public sealed class NoopEvent : IEvent
    {
    }
}
