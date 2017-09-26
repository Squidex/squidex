// ==========================================================================
//  SubscribeMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    [TypeName(nameof(SubscribeMessage))]
    public sealed class SubscribeMessage : IMessage
    {
        public string StreamFilter { get; set; }

        public string Position { get; set; }

        public IActor Parent { get; set; }
    }
}
