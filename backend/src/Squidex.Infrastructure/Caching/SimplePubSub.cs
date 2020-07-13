// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Squidex.Infrastructure.Caching
{
    public sealed class SimplePubSub : IPubSub
    {
        private readonly List<Action<object>> handlers = new List<Action<object>>();

        public void Publish(object message)
        {
            foreach (var handler in handlers)
            {
                handler(message);
            }
        }

        public void Subscribe(Action<object> handler)
        {
            Guard.NotNull(handler, nameof(handler));

            handlers.Add(handler);
        }
    }
}
