// ==========================================================================
//  IEventCatchConsumerControlStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventConsumerInfoRepository
    {
        Task<IReadOnlyList<IEventConsumerInfo>> QueryAsync();

        Task<IEventConsumerInfo> FindAsync(string consumerName);

        Task CreateAsync(string consumerName);

        Task StartAsync(string consumerName);

        Task StopAsync(string consumerName, string error = null);

        Task ResetAsync(string consumerName);

        Task SetLastHandledEventNumberAsync(string consumerName, long eventNumber);
    }
}
