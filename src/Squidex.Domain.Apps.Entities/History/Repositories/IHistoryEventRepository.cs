// ==========================================================================
//  IHistoryEventRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Entities.History.Repositories
{
    public interface IHistoryEventRepository
    {
        Task<IReadOnlyList<IHistoryEventEntity>> QueryByChannelAsync(Guid appId, string channelPrefix, int count);
    }
}
