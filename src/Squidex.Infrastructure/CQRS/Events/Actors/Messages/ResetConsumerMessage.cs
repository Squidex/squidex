// ==========================================================================
//  ResetConsumerMessage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    [TypeName(nameof(ResetConsumerMessage))]
    public sealed class ResetConsumerMessage : IMessage
    {
    }
}
