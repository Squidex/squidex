// ==========================================================================
//  IEventNotifier.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventNotifier
    {
        void NotifyEventsStored(string streamName);

        IDisposable Subscribe(Action<string> handler);
    }
}
