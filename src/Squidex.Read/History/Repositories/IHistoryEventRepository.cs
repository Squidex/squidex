// ==========================================================================
//  IHistoryEventRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Read.History.Repositories
{
    public interface IHistoryEventRepository
    {
        Task<List<IHistoryEventEntity>> FindHistoryByChannel(string channelPrefix, int count);
    }
}
