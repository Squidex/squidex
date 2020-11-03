// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerGrain : IBackgroundGrain
    {
        Task<EventConsumerInfo> GetStateAsync();

        Task<EventConsumerInfo> StopAsync();

        Task<EventConsumerInfo> StartAsync();

        Task<EventConsumerInfo> ResetAsync();
    }
}
