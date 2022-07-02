// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventNotifier
    {
        void NotifyEventsStored(string streamName);
    }

    public sealed class NoopEventNotifier : IEventNotifier
    {
        public void NotifyEventsStored(string streamName)
        {
        }
    }
}
