// ==========================================================================
//  ReceiveEventMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    [TypeName(nameof(ReceiveEventMessage))]
    public sealed class ReceiveEventMessage : IMessage
    {
        public StoredEvent Event { get; set; }

        public IEventSubscription Source { get; set; }
    }
}
