// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public interface IPubSubGrainObserver : IGrainObserver
    {
        void Handle(object message);

        void Subscribe(Action<object> handler);
    }
}
