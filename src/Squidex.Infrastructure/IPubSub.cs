// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Infrastructure
{
    public interface IPubSub
    {
        void Publish<T>(T value, bool notifySelf);

        IDisposable Subscribe<T>(Action<T> handler);
    }
}
