// ==========================================================================
//  StopConsumerMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    [TypeName(nameof(StopConsumerMessage))]
    public sealed class StopConsumerMessage : IMessage
    {
        public Exception Exception { get; set; }
    }
}
