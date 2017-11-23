// ==========================================================================
//  IPubSub.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
