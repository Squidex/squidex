// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventSubscription
    {
        void WakeUp()
        {
        }

        void Unsubscribe();
    }
}