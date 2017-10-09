// ==========================================================================
//  IEventConsumerInfoRepository.cs
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

        Task ClearAsync(IEnumerable<string> currentConsumerNames);

        Task SetAsync(string consumerName, string position, bool isStopped, string error = null);
    }
}
