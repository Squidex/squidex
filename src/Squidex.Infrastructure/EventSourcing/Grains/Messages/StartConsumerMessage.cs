// ==========================================================================
//  StartConsumerMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events.Grains.Messages
{
    public sealed class StartConsumerMessage
    {
        public string ConsumerName { get; set; }
    }
}
