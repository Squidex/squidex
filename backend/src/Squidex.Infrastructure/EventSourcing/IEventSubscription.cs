// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventSubscription
    {
        object? Sender => this;

        void Unsubscribe()
        {
        }

        void WakeUp()
        {
        }
    }
}