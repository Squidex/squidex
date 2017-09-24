// ==========================================================================
//  IEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventNotifier
    {
        void NotifyEventsStored(string streamName);

        IDisposable Subscribe(Action<string> handler);
    }
}
