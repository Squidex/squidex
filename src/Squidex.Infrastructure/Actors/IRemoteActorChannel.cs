// ==========================================================================
//  IRemoteActorChannel.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Actors
{
    public interface IRemoteActorChannel
    {
        Task SendAsync(string recipient, object message);

        void Subscribe(string recipient, Action<object> handler);
    }
}