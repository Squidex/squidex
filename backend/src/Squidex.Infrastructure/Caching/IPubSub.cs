// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Caching
{
    public interface IPubSub
    {
        void Publish(object message);

        void Subscribe(Action<object> handler);
    }
}
