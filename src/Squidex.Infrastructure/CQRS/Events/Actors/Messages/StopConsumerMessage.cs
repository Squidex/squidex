// ==========================================================================
//  StopConsumerMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    public sealed class StopConsumerMessage
    {
        public string ConsumerName { get; set; }
    }
}
