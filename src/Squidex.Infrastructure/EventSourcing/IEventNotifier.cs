// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
