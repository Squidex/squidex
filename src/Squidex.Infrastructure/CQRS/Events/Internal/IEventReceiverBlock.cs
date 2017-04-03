// ==========================================================================
//  IEventReceiverBlock.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events.Internal
{
    public interface IEventReceiverBlock
    {
        Action<Exception> OnError { get; set; }

        void Reset();

        void Stop();
    }
}
