// ==========================================================================
//  GetStatesResponse.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    public sealed class GetStatesResponse
    {
        public EventConsumerInfo[] States { get; set; }
    }
}
