// ==========================================================================
//  IEventSubscription.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public interface IEventSubscription
    {
        Task StopAsync();
    }
}