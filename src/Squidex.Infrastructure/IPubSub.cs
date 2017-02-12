// ==========================================================================
//  IInvalidator.cs
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
        void Publish(string channelName, string token, bool notifySelf);

        IDisposable Subscribe(string channelName, Action<string> handler);
    }
}
